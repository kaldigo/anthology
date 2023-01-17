using Anthology.Plugins.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using static Anthology.Plugins.Models.Metadata;

namespace Anthology.Plugins.LibrarySources
{
    internal class AudiobookShelf : IImportSource
    {
        public string Name => "Audiobook Shelf";
        public string IdentifierKey => "ABSID";

        public List<string> Settings => new List<string>() { "Url", "Bearer", "LibraryID" };

        private static Task _task;

        private static List<AudiobookShelfResult> _bookList;
        private static DateTime _lastUpdated;

        private static Task<string> GetBookList(Dictionary<string, string> settings)
        {
            if (_task != null && (_task.Status == TaskStatus.Running || _task.Status == TaskStatus.WaitingToRun || _task.Status == TaskStatus.WaitingForActivation))
            {
                return Task.FromResult("Import already running");
            }
            else
            {
                _task = Task.Factory.StartNew(() =>
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var url = settings["Url"] + "/api/libraries/" + settings["LibraryID"] + "/items";
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

                _task.Wait();

                return Task.FromResult("Import complete");
            }
        }

        public List<ImportItem> GetImportItems(Dictionary<string, string> settings)
        {
            if (_lastUpdated == null || _lastUpdated.AddMinutes(1) < DateTime.Now) GetBookList(settings);
            if (_bookList == null) return new List<ImportItem>();
            return _bookList.Select(b =>
                {
                    var identifiers = new List<KeyValuePair<string, string>>();
                    if (!string.IsNullOrWhiteSpace(b.media.metadata.isbn)) identifiers.Add(new KeyValuePair<string, string>("ISBN", b.media.metadata.isbn));
                    if (!string.IsNullOrWhiteSpace(b.media.metadata.asin)) identifiers.Add(new KeyValuePair<string, string>("ASIN", b.media.metadata.asin));
                    return new ImportItem()
                    {
                        Key = IdentifierKey,
                        Identifier = b.id,
                        Identifiers = identifiers,
                        Metadata = new Metadata()
                        {
                            Title = b.media.metadata.title,
                            Authors = b.media.metadata.authors.Select(a => a.name).ToList(),
                            Narrators = b.media.metadata.narrators,
                            Series = b.media.metadata.series
                                .Select(s => new MetadataSeries(s.name, s.sequence.ToString())).ToList()
                        }
                    };
                }
            ).ToList();
        }

        #region Import Model
        public class AudiobookShelfItems
        {
            public List<AudiobookShelfResult> results;
            public int total;
            public int limit;
            public int page;
            public bool sortDesc;
            public string mediaType;
            public bool minified;
            public bool collapseseries;
        }
        public class AudiobookShelfResult
        {
            public string id;
            public string ino;
            public string libraryId;
            public string folderId;
            public string path;
            public string relPath;
            public bool isFile;
            public object mtimeMs;
            public object ctimeMs;
            public int birthtimeMs;
            public object addedAt;
            public object updatedAt;
            public object lastScan;
            public string scanVersion;
            public bool isMissing;
            public bool isInvalid;
            public string mediaType;
            public AudiobookShelfMedia media;
            public List<AudiobookShelfLibraryFile> libraryFiles;
        }
        public class AudiobookShelfMedia
        {
            public string libraryItemId;
            public AudiobookShelfMetadata metadata;
            public string coverPath;
            public List<string> tags;
            public List<AudiobookShelfAudioFile> audioFiles;
            public List<AudiobookShelfChapter> chapters;
            public List<object> missingParts;
            public object ebookFile;
        }
        public class AudiobookShelfLibraryFile
        {
            public string ino;
            public AudiobookShelfMetadata metadata;
            public object addedAt;
            public object updatedAt;
            public string fileType;
        }
        public class AudiobookShelfMetadata
        {
            public string title;
            public string subtitle;
            public List<AudiobookShelfAuthor> authors;
            public List<string> narrators;
            public List<AudiobookShelfSeries> series;
            public List<string> genres;
            public string publishedYear;
            public object publishedDate;
            public string publisher;
            public string description;
            public string isbn;
            public string asin;
            public string language;
            public bool @explicit;
            public string filename;
            public string ext;
            public string path;
            public string relPath;
            public int size;
            public object mtimeMs;
            public object ctimeMs;
            public int birthtimeMs;
        }
        public class AudiobookShelfAudioFile
        {
            public int index;
            public string ino;
            public AudiobookShelfMetadata metadata;
            public object addedAt;
            public object updatedAt;
            public int? trackNumFromMeta;
            public object discNumFromMeta;
            public int? trackNumFromFilename;
            public object discNumFromFilename;
            public bool manuallyVerified;
            public bool invalid;
            public bool exclude;
            public object error;
            public string format;
            public double duration;
            public int bitRate;
            public string language;
            public string codec;
            public string timeBase;
            public int channels;
            public string channelLayout;
            public List<AudiobookShelfChapter> chapters;
            public string embeddedCoverArt;
            public AudiobookShelfMetaTags metaTags;
            public string mimeType;
        }
        public class AudiobookShelfChapter
        {
            public int id;
            public double start;
            public double end;
            public string title;
        }
        public class AudiobookShelfAuthor
        {
            public string id;
            public string name;
        }
        public class AudiobookShelfSeries
        {
            public string id;
            public string name;
            public float? sequence;
        }
        public class AudiobookShelfMetaTags
        {
            public string tagAlbum;
            public string tagArtist;
            public string tagTitle;
            public string tagTrack;
            public string tagAlbumArtist;
            public string tagDate;
            public string tagComment;
            public string tagEncoder;
            public string tagGenre;
        }
        #endregion
    }
}
