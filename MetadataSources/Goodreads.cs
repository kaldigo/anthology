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

        public List<string> Settings => new List<string>() { };

        public Metadata GetMetadata(string identifier, Dictionary<string, string> settings)
        {
            try
            {
                return GetBookMetadata(identifier);
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
                    var url = "https://www.goodreads.com/search?q=" + title;
                    if (author != null) url = url + " " + author;

                    var request = new HttpRequestMessage(HttpMethod.Post, "https://biblioreads.kaldigo.co.uk/api/search/books");
                    var content = new StringContent("{\"queryURL\":\"" + url + "\"}", null, "application/json");
                    request.Content = content;

                    var response = client.Send(request);


                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("Book Not Found: " + url);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            throw new Exception("Bad Request: " + url);
                        }
                        else
                        {
                            throw new Exception("Unexpected response fetching book data");
                        }
                    }

                    var search = JsonConvert.DeserializeObject<GoodreadsSearch>(response.Content.ReadAsStringAsync().Result);

                    var results = search.result.Select(r =>
                    {
                        var book = GetBook(r.bookURL);
                        var metadata = new Metadata();

                        metadata.Title = book.title;
                        metadata.Authors.AddRange(book.author.Where(a => !a.name.Contains("Narrator") && !a.name.Contains("Illustrator") && !a.name.Contains("Editor") && !a.name.Contains("Transilator")).Select(a => a.name.Trim()));
                        if (!String.IsNullOrEmpty(book.series)) metadata.Series.Add(ParseSeries(book.series));
                        metadata.Description = book.desc;
                        metadata.PublishDate = ParseDate(book.publishDate.Replace("\\n", "").Trim(), book.publishDate.Replace("\\n", "").Trim());
                        metadata.Publisher = ParsePublisher(book.publishDate.Replace("\\n", "").Trim(), book.publishDate.Replace("\\n", "").Trim());
                        metadata.Genres.AddRange(book.genres.Where(g => !String.IsNullOrEmpty(g) && !metadata.Genres.Any(mg => mg == g)));
                        metadata.IsExplicit = metadata.Genres.Any(g => g.ToLower() is "adult" or "erotic" or "erotica");
                        metadata.Covers.Add(book.cover);

                        var identifierRegex = new Regex(@"(?<identifier>\d+)$");
                        var identifierMatch = identifierRegex.Match(book.quotesURL);

                        return new MetadataSearchResult()
                        {
                            Key = IdentifierKey,
                            Identifier = identifierMatch.Groups["identifier"].Value.Trim(),
                            Metadata = metadata
                        };
                    }).ToList();

                    return results;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private Metadata GetBookMetadata(string identifier)
        {
            var work = GetWork(identifier);

            var metadata = new Metadata();

            foreach (var edition in work.editions)
            {
                var book = GetBook(edition.url);

                if (metadata.Title == null) metadata.Title = book.title;
                if (metadata.Authors.Count() == 0) metadata.Authors.AddRange(book.author.Where(a => !a.name.Contains("Narrator") && !a.name.Contains("Illustrator") && !a.name.Contains("Editor") && !a.name.Contains("Transilator")).Select(a => a.name.Trim()));
                if (metadata.Series.Count() == 0 && !String.IsNullOrEmpty(book.series)) metadata.Series.Add(ParseSeries(book.series));
                if (metadata.Description == null) metadata.Description = book.desc;
                if (!metadata.PublishDate.HasValue) metadata.PublishDate = ParseDate(work.firstPublished.Replace("\\n", "").Trim(), work.publishDate.Replace("\\n", "").Trim());
                if (metadata.Publisher == null) metadata.Publisher = ParsePublisher(work.firstPublished.Replace("\\n", "").Trim(), work.publishDate.Replace("\\n", "").Trim());
                metadata.Genres.AddRange(book.genres.Where(g => !String.IsNullOrEmpty(g) && !metadata.Genres.Any(mg => mg == g)));
                if (metadata.Language == null) metadata.Language = ParseLanguage(edition.editionLanguage.Replace("\\n", ""));
                metadata.IsExplicit = metadata.Genres.Any(g => g.ToLower() is "adult" or "erotic" or "erotica");
                if (book.cover != null) metadata.Covers.Add(book.cover);
            }

            return metadata;
        }

        private Work GetWork(string identifier)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://biblioreads.kaldigo.co.uk/api/works/editions");
            var content = new StringContent("{\"queryURL\":\"http://www.goodreads.com/work/editions/" + identifier + "\"}", null, "application/json");
            request.Content = content;

            var response = client.Send(request);


            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception("Work Not Found: " + identifier);
            }

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new Exception("Bad Request: " + identifier);
                }
                else
                {
                    throw new Exception("Unexpected response fetching book data");
                }
            }

            var work = JsonConvert.DeserializeObject<Work>(response.Content.ReadAsStringAsync().Result);

            return work;
        }

        private Book GetBook(string url)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://biblioreads.kaldigo.co.uk/api/book-scraper");
            var content = new StringContent("{\"queryURL\":\"https://www.goodreads.com" + url + "\"}", null, "application/json");
            request.Content = content;

            var response = client.Send(request);


            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception("Book Not Found: " + url);
            }

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new Exception("Bad Request: " + url);
                }
                else
                {
                    throw new Exception("Unexpected response fetching book data");
                }
            }

            var book = JsonConvert.DeserializeObject<Book>(response.Content.ReadAsStringAsync().Result);

            return book;

        }

        private Metadata.MetadataSeries ParseSeries(string series)
        {
            var seriesRegex = new Regex(@"^(?<series>.+)?\s#(?<volume>\d+)$");
            var match = seriesRegex.Match(series);
            if (match.Success)
            {
                return new Metadata.MetadataSeries(match.Groups["series"].Value, match.Groups["volume"].Value);
            }
            else
            {
                return new Metadata.MetadataSeries(series, null);
            }
        }

        private DateTime? ParseDate(string date1, string date2)
        {
            var date = String.IsNullOrEmpty(date1) ? date2 : date1;
            if (date == null) return null;

            var dayRegex = new Regex(@"(?<day>\d\d?)(?:st|nd|rd|th|,)");
            var monthRegex = new Regex(@"(?<month>January|February|March|April|May|June|July|August|September|October|November)");
            var yearRegex = new Regex(@"(?<year>\d{4})");
            var matchDay = dayRegex.Match(date);
            var matchMonth = monthRegex.Match(date);
            var matchYear = yearRegex.Match(date);

            var day = matchDay.Success ? int.Parse(matchDay.Groups["day"].Value) : 1;
            var monthString = matchMonth.Success ? matchMonth.Groups["month"].Value : null;
            var month = 0;
            var year = matchYear.Success ? int.Parse(matchYear.Groups["year"].Value) : 0;

            switch (monthString)
            {
                case "January":
                    month = 1;
                    break;
                case "February":
                    month = 2;
                    break;
                case "March":
                    month = 3;
                    break;
                case "April":
                    month = 4;
                    break;
                case "May":
                    month = 5;
                    break;
                case "June":
                    month = 6;
                    break;
                case "July":
                    month = 7;
                    break;
                case "August":
                    month = 8;
                    break;
                case "September":
                    month = 9;
                    break;
                case "October":
                    month = 10;
                    break;
                case "November":
                    month = 11;
                    break;
                case "December":
                    month = 12;
                    break;
            }

            if (year == 0) return null;
            if (month == 0) return new DateTime(year);
            return new DateTime(year, month, day);

        }
        private string ParsePublisher(string date1, string date2)
        {
            var date = String.IsNullOrEmpty(date1) ? date2 : date1;
            if (date == null) return null;

            var publisherRegex = new Regex(@"by(?<publisher>.+)");
            var match = publisherRegex.Match(date);

            if (match.Success)
            {
                return match.Groups["publisher"].Value.Trim();
            }
            else
            {
                return null;
            }
        }

        private string ParseLanguage(string language)
        {
            switch (language.ToLower().Trim())
            {
                case "en-us":
                case "en-gb":
                case "eng":
                case "english":
                    return "English";
                default:
                    return language.Replace("\\n", "").Trim();
            }
        }

        #region Import Model
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Edition
        {
            public int id { get; set; }
            public string cover { get; set; }
            public string title { get; set; }
            public string url { get; set; }
            public string publishDate { get; set; }
            public string editionInfo { get; set; }
            public string rating { get; set; }
            public string ISBN { get; set; }
            public string ASIN { get; set; }
            public string editionLanguage { get; set; }
        }

        public class Work
        {
            public string status { get; set; }
            public string source { get; set; }
            public string scrapeURL { get; set; }
            public string book { get; set; }
            public string bookURL { get; set; }
            public string author { get; set; }
            public string authorURL { get; set; }
            public string publishDate { get; set; } = "";
            public string firstPublished { get; set; } = "";
            public List<Edition> editions { get; set; }
            public DateTime lastScraped { get; set; }
        }
        public class Author
        {
            public int id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
        }

        public class Review
        {
            public int id { get; set; }
            public string image { get; set; }
            public string author { get; set; }
            public string date { get; set; }
            public string text { get; set; }
            public string likes { get; set; }
            public string stars { get; set; }
        }

        public class ReviewBreakdown
        {
            public string rating5 { get; set; }
            public string rating4 { get; set; }
            public string rating3 { get; set; }
            public string rating2 { get; set; }
            public string rating1 { get; set; }
        }

        public class Book
        {
            public string status { get; set; }
            public int statusCode { get; set; }
            public string source { get; set; }
            public string scrapeURL { get; set; }
            public string cover { get; set; }
            public string series { get; set; }
            public string workURL { get; set; }
            public string title { get; set; }
            public List<Author> author { get; set; }
            public string rating { get; set; }
            public string ratingCount { get; set; }
            public string reviewsCount { get; set; }
            public string desc { get; set; }
            public List<string> genres { get; set; }
            public string bookEdition { get; set; }
            public string publishDate { get; set; }
            public List<object> related { get; set; }
            public ReviewBreakdown reviewBreakdown { get; set; }
            public List<Review> reviews { get; set; }
            public string quotes { get; set; }
            public string quotesURL { get; set; }
            public string questions { get; set; }
            public string questionsURL { get; set; }
            public DateTime lastScraped { get; set; }
        }

        public class Result
        {
            public int id { get; set; }
            public string cover { get; set; }
            public string title { get; set; }
            public string bookURL { get; set; }
            public string author { get; set; }
            public string authorURL { get; set; }
            public string rating { get; set; }
        }

        public class GoodreadsSearch
        {
            public string status { get; set; }
            public string source { get; set; }
            public string scrapeURL { get; set; }
            public string searchType { get; set; }
            public string numberOfResults { get; set; }
            public List<Result> result { get; set; }
            public DateTime lastScraped { get; set; }
        }

        #endregion
    }
}
