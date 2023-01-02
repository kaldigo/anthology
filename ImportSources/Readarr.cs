using Anthology.Plugins;
using Anthology.Plugins.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Plugins.ImportSources
{
    internal class Readarr : IImportSource
    {
        public string Name => "Readarr";

        public string IdentifierKey => "GRID";

        public List<string> Settings => new List<string>() { "Url", "Bearer" };

        public List<Import> RunImport(Dictionary<string, string> settings)
        {
            using (HttpClient client = new HttpClient())
            {
                var url = settings["Url"] + "/api/v1/book";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings["Bearer"]);
                var response = client.GetStringAsync(url).Result;
                if (response != null)
                {
                    var jsonString = JsonConvert.DeserializeObject<List<ReadarrBook>>(response);
                    return jsonString.Where(b => b.monitored).Select(b => ConvertReadarrToImport(b)).ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        private Import ConvertReadarrToImport(ReadarrBook readarrBook)
        {
            var import = new Import();
            import.Key = IdentifierKey;
            import.Identifier = readarrBook.foreignBookId;
            import.Title = readarrBook.title;
            import.Authors = new List<string>() { { readarrBook.author.authorName } };

            return import;
        }

        #region Import Model
        private class ReadarrBook
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
            public ReadarrRatings ratings { get; set; }
            public DateTime releaseDate { get; set; }
            public int pageCount { get; set; }
            public List<string> genres { get; set; }
            public ReadarrAuthor author { get; set; }
            public List<ReadarrImage> images { get; set; }
            public List<ReadarrLink> links { get; set; }
            public ReadarrStatistics statistics { get; set; }
            public DateTime added { get; set; }
            public List<ReadarrEdition> editions { get; set; }
            public bool grabbed { get; set; }
            public int id { get; set; }
        }
        private class ReadarrRatings
        {
            public int votes { get; set; }
            public double value { get; set; }
            public double popularity { get; set; }
        }
        private class ReadarrAuthor
        {
            public int authorMetadataId { get; set; }
            public string status { get; set; }
            public bool ended { get; set; }
            public string authorName { get; set; }
            public string authorNameLastFirst { get; set; }
            public string foreignAuthorId { get; set; }
            public string titleSlug { get; set; }
            public string overview { get; set; }
            public List<ReadarrLink> links { get; set; }
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
            public ReadarrRatings ratings { get; set; }
            public ReadarrStatistics statistics { get; set; }
            public int id { get; set; }
        }
        private class ReadarrLink
        {
            public string url { get; set; }
            public string name { get; set; }
        }
        private class ReadarrStatistics
        {
            public int bookFileCount { get; set; }
            public int bookCount { get; set; }
            public int availableBookCount { get; set; }
            public int totalBookCount { get; set; }
            public int sizeOnDisk { get; set; }
            public int percentOfBooks { get; set; }
        }
        private class ReadarrImage
        {
            public string url { get; set; }
            public string coverType { get; set; }
            public string extension { get; set; }
        }
        private class ReadarrEdition
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
            public List<ReadarrImage> images { get; set; }
            public List<ReadarrLink> links { get; set; }
            public ReadarrRatings ratings { get; set; }
            public bool monitored { get; set; }
            public bool manualAdd { get; set; }
            public bool grabbed { get; set; }
            public int id { get; set; }
            public string language { get; set; }
        }
        #endregion
    }
}
