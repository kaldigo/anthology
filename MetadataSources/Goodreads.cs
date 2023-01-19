using Anthology.Plugins;
using Anthology.Plugins.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Anthology.Plugins.MetadataSources.Goodreads;
using System.Reflection.PortableExecutable;
using Newtonsoft.Json.Linq;

namespace Anthology.Plugins.MetadataSources
{
    internal class Goodreads : IMetadataSource
    {
        public string Name => "Goodreads";

        public string IdentifierKey => "GRID";

        public List<string> Settings => new List<string>() {};

        public Metadata GetMetadata(string identifier, Dictionary<string, string> settings)
        {
            try
            {
                return PollBook(identifier);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<MetadataSearchResult> Search(Dictionary<string, string> settings, string title, string author = null)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var url = "https://www.goodreads.com/book/auto_complete?format=json&q=" + title;
                    if (author != null) url = url + " " + author;

                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36");

                    var response = client.SendAsync(request).Result;
                    if (response != null)
                    {
                        var jsonString = JsonConvert.DeserializeObject<List<SearchJsonResource>>(response.Content.ReadAsStringAsync().Result);
                        return jsonString.Select(r => new MetadataSearchResult(){Key = IdentifierKey, Identifier = r.WorkId.ToString(), Metadata = PollBook(r.WorkId.ToString())}).ToList();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private Metadata MapMetadata(WorkResource book)
        {
            var metadata = new Metadata();
            var editionsByPopularity = book.Books.OrderByDescending(e => (double)((decimal)e.AverageRating * e.RatingCount));
            var mostPopularEdition = editionsByPopularity.FirstOrDefault();
            metadata.Title = mostPopularEdition.Title;
            metadata.Authors = book.Authors.Where(a => mostPopularEdition.Contributors.Where(c => c.Role == "Author").Select(c => c.ForeignId).Contains(a.ForeignId)).Select(a => a.Name).ToList();

            if (book.Series != null && book.Series.Count != 0 )
            {
                metadata.Series = book.Series.Select(s =>
                {
                    var title = s.Title;
                    var linkItem = s.LinkItems.FirstOrDefault(i => i.ForeignWorkId == book.ForeignId && i.Primary);
                    var position = "";
                    if (linkItem != null) position = linkItem.PositionInSeries;
                    return new Metadata.MetadataSeries(title, position);
                }).ToList();
            }

            metadata.Description = mostPopularEdition.Description;
            metadata.Publisher = mostPopularEdition.Publisher;
            metadata.PublishDate = mostPopularEdition.ReleaseDate;
            metadata.Genres = book.Genres;
            metadata.Covers = editionsByPopularity.Select(e => e.ImageUrl).Take(10).ToList();

            if (!string.IsNullOrWhiteSpace(mostPopularEdition.Language))
            {
                switch (mostPopularEdition.Language.ToLower().Trim())
                {
                    case "eng":
                    case "english":
                        metadata.Language = "English";
                        break;
                    default:
                        metadata.Language = mostPopularEdition.Language;
                        break;
                }
            }

            metadata.IsExplicit = book.Genres.Any(g => g is "adult" or "erotic" or "erotica");

            return metadata;
        }

        private Metadata PollBook(string foreignBookId)
        {
            WorkResource resource = null;

            HttpClient httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });

            for (var i = 0; i < 60; i++)
            {
                var url = "https://api.bookinfo.club/v1/work/" + foreignBookId;

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36");

                var httpResponse = httpClient.Send(request);

                if (httpResponse.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    WaitUntilRetry(httpResponse);
                    continue;
                }

                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new Exception("Book Not Found: " + foreignBookId);
                }

                if (httpResponse.StatusCode == HttpStatusCode.Redirect)
                {
                    var location = httpResponse.Headers.Location.OriginalString;
                    var split = location.Split('/').Reverse().ToList();
                    var newId = split[0];
                    var type = split[1];

                    if (type == "author")
                    {
                        url = "https://api.bookinfo.club/v1/author/" + newId;
                        
                        request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36");

                        httpResponse = httpClient.Send(request);

                        var author = JsonConvert.DeserializeObject<AuthorResource>(httpResponse.Content.ReadAsStringAsync().Result);
                        var authorBook = author.Works.SingleOrDefault(x => x.ForeignId.ToString() == foreignBookId);

                        if (authorBook == null)
                        {
                            throw new Exception("Book Not Found: " + foreignBookId);
                        }
                        
                        authorBook.Authors = new List<AuthorResource>(){author};
                        var authorBookSeries = author.Series.Where(s =>
                            s.LinkItems.Any(i => i.ForeignWorkId.ToString() == foreignBookId)).ToList();
                        if (author.Series.Any() && !authorBook.Series.Any() && authorBookSeries.Any())
                            authorBook.Series = authorBookSeries;

                        httpClient.Dispose();
                        
                        return MapMetadata(authorBook);
                    }
                    else
                    {
                        throw new NotImplementedException($"Unexpected response from {httpResponse.RequestMessage.RequestUri}");
                    }
                }

                if (!httpResponse.IsSuccessStatusCode)
                {
                    if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new Exception("Bad Request: " + foreignBookId);
                    }
                    else
                    {
                        throw new Exception("Unexpected response fetching book data");
                    }
                }
                
                resource = JsonConvert.DeserializeObject<WorkResource>(httpResponse.Content.ReadAsStringAsync().Result);

                if (resource.Books != null)
                {
                    break;
                }

                Thread.Sleep(2000);
            }

            if (resource?.Books == null || resource?.Authors == null || (!resource?.Authors?.Any() ?? false))
            {
                throw new Exception($"Failed to get books for {foreignBookId}");
            }

            var book = MapMetadata(resource);

            httpClient.Dispose();

            return book;
        }

        private void WaitUntilRetry(HttpResponseMessage response)
        {
            var seconds = 5;

            if (response.Headers.RetryAfter.Delta.HasValue)
            {
                seconds = (int)response.Headers.RetryAfter.Delta.Value.TotalSeconds;
            }

            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }

        #region Import Model

        public class WorkResource
        {
            public int ForeignId { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }
            public DateTime? ReleaseDate { get; set; }
            public List<string> Genres { get; set; }
            public List<int> RelatedWorks { get; set; }
            public List<BookResource> Books { get; set; }
            public List<SeriesResource> Series { get; set; } = new List<SeriesResource>();
            public List<AuthorResource> Authors { get; set; } = new List<AuthorResource>();
        }

        public class BookResource
        {
            public int ForeignId { get; set; }
            public string Asin { get; set; }
            public string Description { get; set; }
            public string Isbn13 { get; set; }
            public string Title { get; set; }
            public string Language { get; set; }
            public string Format { get; set; }
            public string EditionInformation { get; set; }
            public string Publisher { get; set; }
            public string ImageUrl { get; set; }
            public bool IsEbook { get; set; }
            public int? NumPages { get; set; }
            public int RatingCount { get; set; }
            public double AverageRating { get; set; }
            public string Url { get; set; }
            public DateTime? ReleaseDate { get; set; }

            public List<ContributorResource> Contributors { get; set; } = new List<ContributorResource>();
        }

        public class SeriesResource
        {
            public int ForeignId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }

            public List<SeriesWorkLinkResource> LinkItems { get; set; }
        }

        public class SeriesWorkLinkResource
        {
            public int ForeignWorkId { get; set; }
            public string PositionInSeries { get; set; }
            public int SeriesPosition { get; set; }
            public bool Primary { get; set; }
        }

        public class AuthorResource
        {
            public int ForeignId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public string Url { get; set; }
            public int RatingCount { get; set; }
            public double AverageRating { get; set; }
            public List<WorkResource> Works { get; set; }
            public List<SeriesResource> Series { get; set; }
        }

        public class ContributorResource
        {
            public int ForeignId { get; set; }
            public string Role { get; set; }
        }

        public class SearchJsonResource
        {
            [JsonProperty("imageUrl")]
            public string ImageUrl { get; set; }

            [JsonProperty("bookId")]
            public int BookId { get; set; }

            [JsonProperty("workId")]
            public int WorkId { get; set; }

            [JsonProperty("bookUrl")]
            public string BookUrl { get; set; }

            [JsonProperty("from_search")]
            public bool FromSearch { get; set; }

            [JsonProperty("from_srp")]
            public bool FromSrp { get; set; }

            [JsonProperty("qid")]
            public string Qid { get; set; }

            [JsonProperty("rank")]
            public int Rank { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("bookTitleBare")]
            public string BookTitleBare { get; set; }

            [JsonProperty("numPages")]
            public int? PageCount { get; set; }

            [JsonProperty("avgRating")]
            public decimal AverageRating { get; set; }

            [JsonProperty("ratingsCount")]
            public int RatingsCount { get; set; }

            [JsonProperty("author")]
            public AuthorJsonResource Author { get; set; }

            [JsonProperty("kcrPreviewUrl")]
            public string KcrPreviewUrl { get; set; }

            [JsonProperty("description")]
            public DescriptionJsonResource Description { get; set; }
        }

        public class AuthorJsonResource
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("isGoodreadsAuthor")]
            public bool IsGoodreadsAuthor { get; set; }

            [JsonProperty("profileUrl")]
            public string ProfileUrl { get; set; }

            [JsonProperty("worksListUrl")]
            public string WorksListUrl { get; set; }
        }

        public class DescriptionJsonResource
        {
            [JsonProperty("html")]
            public string Html { get; set; }

            [JsonProperty("truncated")]
            public bool Truncated { get; set; }

            [JsonProperty("fullContentUrl")]
            public string FullContentUrl { get; set; }
        }

        #endregion
    }
}
