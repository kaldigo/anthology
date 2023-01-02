using Anthology.Plugins;
using Anthology.Plugins.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Anthology.Plugins.MetadataSources
{
    internal class Goodreads : IMetadataSource
    {
        public string Name => "Goodreads";

        public string IdentifierKey => "GRID";

        public List<string> Settings => new List<string>() { "ReadarrUrl", "Bearer" };

        public Metadata GetMetadata(string identifier, Dictionary<string, string> settings)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var url = settings["ReadarrUrl"] + "/api/v1/book/lookup?term=work:" + identifier;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings["Bearer"]);
                    var response = client.GetStringAsync(url).Result;
                    if (response != null)
                    {
                        var jsonString = JsonConvert.DeserializeObject<List<GoodreadsBook>>(response);
                        return ConvertImportToMetadata(jsonString[0]);
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

        public List<Metadata> Search(Dictionary<string, string> settings, string title, string author = null)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var url = settings["ReadarrUrl"] + "/api/v1/book/lookup?term=" + title;
                    if (author != null) url = url + " " + author;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings["Bearer"]);
                    var response = client.GetStringAsync(url).Result;
                    if (response != null)
                    {
                        var jsonString = JsonConvert.DeserializeObject<List<GoodreadsBook>>(response);
                        return jsonString.Select(r => ConvertImportToMetadata(r)).ToList();
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
        private static List<Metadata.MetadataSeries> ExtractSeries(List<string> seriesList)
        {
            var seriesDict = new List<Metadata.MetadataSeries>();
            foreach (var item in seriesList)
            {
                var series = Regex.Match(item.Trim(), @"(.+) #(.+)").Groups[1].Value.Trim();
                var indexString = Regex.Match(item.Trim(), @"(.+) #(.+)").Groups[2].Value.Trim();

                if (string.IsNullOrEmpty(series)) series = item.Trim();
                float indexFloat;
                float.TryParse(indexString, out indexFloat);
                if (indexFloat == null) indexFloat = 0;
                var bookSeries = new Metadata.MetadataSeries(series, indexFloat.ToString());

                seriesDict.Add(bookSeries);
            }
            return seriesDict;
        }

        private Metadata ConvertImportToMetadata(GoodreadsBook goodreadsBook)
        {
            var metadata = new Metadata();
            metadata.Title = goodreadsBook.title;
            metadata.Authors = new List<string>() { { goodreadsBook.author.authorName } };
            if (!string.IsNullOrWhiteSpace(goodreadsBook.seriesTitle)) metadata.Series = ExtractSeries(goodreadsBook.seriesTitle.Split(";").ToList());
            metadata.Description = goodreadsBook.overview;
            metadata.Publisher = goodreadsBook.editions[0].publisher;
            metadata.PublishDate = goodreadsBook.releaseDate;
            metadata.Genres = goodreadsBook.genres;
            metadata.Covers = goodreadsBook.editions.SelectMany(e => e.images.Select(i => i.url)).Take(10).ToList();

            return metadata;
        }

        #region Import Model
        private class GoodreadsBook
        {
            public string title { get; set; }
            public string authorTitle { get; set; }
            public string seriesTitle { get; set; }
            public string disambiguation { get; set; }
            public string overview { get; set; }
            public int authorId { get; set; }
            public string foreignBookId { get; set; }
            public string titleSlug { get; set; }
            public bool monitored { get; set; }
            public bool anyEditionOk { get; set; }
            public GoodreadsRatings ratings { get; set; }
            public DateTime releaseDate { get; set; }
            public int pageCount { get; set; }
            public List<string> genres { get; set; }
            public GoodreadsAuthor author { get; set; }
            public List<GoodreadsImage> images { get; set; }
            public List<GoodreadsLink> links { get; set; }
            public GoodreadsStatistics statistics { get; set; }
            public DateTime added { get; set; }
            public List<GoodreadsEdition> editions { get; set; }
            public bool grabbed { get; set; }
            public int id { get; set; }
        }
        private class GoodreadsRatings
        {
            public int votes { get; set; }
            public double value { get; set; }
            public double popularity { get; set; }
        }
        private class GoodreadsAuthor
        {
            public int authorMetadataId { get; set; }
            public string status { get; set; }
            public bool ended { get; set; }
            public string authorName { get; set; }
            public string authorNameLastFirst { get; set; }
            public string foreignAuthorId { get; set; }
            public string titleSlug { get; set; }
            public string overview { get; set; }
            public List<GoodreadsLink> links { get; set; }
            public List<object> images { get; set; }
            public string path { get; set; }
            public int qualityProfileId { get; set; }
            public int metadataProfileId { get; set; }
            public bool monitored { get; set; }
            public string monitorNewItems { get; set; }
            public List<object> genres { get; set; }
            public string cleanName { get; set; }
            public string sortName { get; set; }
            public string sortNameLastFirst { get; set; }
            public List<object> tags { get; set; }
            public DateTime added { get; set; }
            public GoodreadsRatings ratings { get; set; }
            public GoodreadsStatistics statistics { get; set; }
            public int id { get; set; }
        }
        private class GoodreadsLink
        {
            public string url { get; set; }
            public string name { get; set; }
        }
        private class GoodreadsStatistics
        {
            public int bookFileCount { get; set; }
            public int bookCount { get; set; }
            public int availableBookCount { get; set; }
            public int totalBookCount { get; set; }
            public int sizeOnDisk { get; set; }
            public int percentOfBooks { get; set; }
        }
        private class GoodreadsImage
        {
            public string url { get; set; }
            public string coverType { get; set; }
            public string extension { get; set; }
        }
        private class GoodreadsEdition
        {
            public int bookId { get; set; }
            public string foreignEditionId { get; set; }
            public string titleSlug { get; set; }
            public string isbn13 { get; set; }
            public string asin { get; set; }
            public string title { get; set; }
            public string overview { get; set; }
            public string format { get; set; }
            public bool isEbook { get; set; }
            public string disambiguation { get; set; }
            public string publisher { get; set; }
            public int pageCount { get; set; }
            public DateTime releaseDate { get; set; }
            public List<GoodreadsImage> images { get; set; }
            public List<GoodreadsLink> links { get; set; }
            public GoodreadsRatings ratings { get; set; }
            public bool monitored { get; set; }
            public bool manualAdd { get; set; }
            public bool grabbed { get; set; }
            public int id { get; set; }
            public string language { get; set; }
        }
        #endregion
    }
}
