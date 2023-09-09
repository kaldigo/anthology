using Anthology.Plugins;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Anthology.Plugins.Models;
using static Anthology.Plugins.Models.Metadata;

namespace Anthology.Plugins.LibrarySources
{
    internal class AudiobookShelf : ILibrarySource
    {
        public string Name => "Audiobook Shelf";

        public List<string> Settings => new List<string>() { "Url", "Bearer", "LibraryID" };

        private static Task _task;

        private static List<AudiobookShelfResult> _bookList;
        private static DateTime _lastUpdated;

        private static Task<string> GetBookList(Dictionary<string, string> settings)
        {
            if (!(_task != null && (_task.Status == TaskStatus.Running || _task.Status == TaskStatus.WaitingToRun || _task.Status == TaskStatus.WaitingForActivation)))
            {
                _task = Task.Factory.StartNew(() =>
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var url = settings["Url"] + "/api/libraries/" + settings["LibraryID"] + "/items?limit=-1";
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings["Bearer"]);
                        var response = client.GetStringAsync(url).Result;
                        if (response != null)
                        {
                            var jsonString = JsonConvert.DeserializeObject<AudiobookShelfItems>(response);
                            if (jsonString != null)
                            {
                                _bookList = jsonString.results;
                                _lastUpdated = DateTime.Now;
                            }
                        }
                    }
                });
            }
            _task.Wait();
            return Task.FromResult("Import complete");
        }

        private List<AudiobookShelfResult> GetLibraryImport(Dictionary<string, string> settings)
        {
            if (_lastUpdated == null || _lastUpdated.AddMinutes(1) < DateTime.Now) GetBookList(settings);
            if (_bookList == null) return null;
            return _bookList;
        }

        public AudiobookShelfResult GetBookDetails(string isbn, Dictionary<string, string> settings)
        {
            var boolList = GetLibraryImport(settings);
            return boolList == null ? null : boolList.FirstOrDefault(b => b.media.metadata.isbn == isbn);
        }

        public List<string> GetLibraryItemList(Dictionary<string, string> settings)
        {
            var boolList = GetLibraryImport(settings);
            return boolList == null ? null : boolList.Select(b => b.media.metadata.isbn).Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
        }

        public bool IsBookInLibrary(string isbn, Dictionary<string, string> settings)
        {
            var libraryBook = GetBookDetails(isbn, settings);
            if (libraryBook == null) return false;
            return true;
        }

        #region Import Model
        public class AudiobookShelfMedia
        {
            public string id { get; set; }
            public AudiobookShelfMetadata metadata { get; set; }
            public string coverPath { get; set; }
            public List<string> tags { get; set; }
            public int numTracks { get; set; }
            public int numAudioFiles { get; set; }
            public int numChapters { get; set; }
            public int numMissingParts { get; set; }
            public int numInvalidAudioFiles { get; set; }
            public double duration { get; set; }
            public int size { get; set; }
            public object ebookFormat { get; set; }
        }

        public class AudiobookShelfMetadata
        {
            public string title { get; set; }
            public string titleIgnorePrefix { get; set; }
            public string subtitle { get; set; }
            public string authorName { get; set; }
            public string authorNameLF { get; set; }
            public string narratorName { get; set; }
            public string seriesName { get; set; }
            public List<string> genres { get; set; }
            public string publishedYear { get; set; }
            public object publishedDate { get; set; }
            public string publisher { get; set; }
            public string description { get; set; }
            public string isbn { get; set; }
            public string asin { get; set; }
            public string language { get; set; }
            public bool @explicit { get; set; }
            public bool abridged { get; set; }
        }

        public class AudiobookShelfResult
        {
            public string id { get; set; }
            public string ino { get; set; }
            public string oldLibraryItemId { get; set; }
            public string libraryId { get; set; }
            public string folderId { get; set; }
            public string path { get; set; }
            public string relPath { get; set; }
            public bool isFile { get; set; }
            public object mtimeMs { get; set; }
            public object ctimeMs { get; set; }
            public int birthtimeMs { get; set; }
            public object addedAt { get; set; }
            public object updatedAt { get; set; }
            public bool isMissing { get; set; }
            public bool isInvalid { get; set; }
            public string mediaType { get; set; }
            public AudiobookShelfMedia media { get; set; }
            public int numFiles { get; set; }
            public int size { get; set; }
        }

        public class AudiobookShelfItems
        {
            public List<AudiobookShelfResult> results { get; set; }
            public int total { get; set; }
            public int limit { get; set; }
            public int page { get; set; }
            public bool sortDesc { get; set; }
            public string mediaType { get; set; }
            public bool minified { get; set; }
            public bool collapseseries { get; set; }
            public string include { get; set; }
            public int offset { get; set; }
        }


        #endregion
    }
}
