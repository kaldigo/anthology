using Anthology.Data.AudiobookShelf;
using Anthology.Data.Readarr;
using Anthology.Utils;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Anthology.Services
{
    public interface IAudiobookShelfService
    {
        Result GetBookDetails(string isbn);
        bool IsBookInLibrary(string isbn);
        public IEnumerable<Result> GetMissingBooks(IEnumerable<string> isbnList);
    }
    public class AudiobookShelfService : IAudiobookShelfService
    {
        private static Task _task;

        private static List<Result> _bookList;
        private static DateTime _lastUpdated;

        private static Task<string> GetBookList()
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
                        var url = Environment.GetEnvironmentVariable("AUDIOBOOKSHELF_URL") + "/api/libraries/lib_gxpan2bkqvo6rt8nxg/items";
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("AUDIOBOOKSHELF_TOKEN"));
                        var response = client.GetStringAsync(url).Result;
                        if (response != null)
                        {
                            var jsonString = JsonConvert.DeserializeObject<Items>(response);
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

        public Result GetBookDetails(string isbn)
        {
            if (_lastUpdated == null || _lastUpdated.AddMinutes(1) < DateTime.Now) GetBookList();
            if(_bookList == null) return null;
            return _bookList.FirstOrDefault(b => b.media.metadata.isbn == isbn);
        }
        public bool IsBookInLibrary(string isbn)
        {
            var libraryBook = GetBookDetails(isbn);
            if (libraryBook == null) return false;
            return true;
        }
        public IEnumerable<Result> GetMissingBooks(IEnumerable<string> isbnList)
        {
            return _bookList.Where(b => !isbnList.Contains(b.media.metadata.isbn));
        }
    }
}
