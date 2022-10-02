using Anthology.Data.DB;
using Anthology.Utils;
using HtmlAgilityPack;
using System.IO.Compression;
using System.Net;
using Anthology.Utils;

namespace Anthology.Services
{
    public interface IBookFunnelService
    {
        List<BookFunnel> GetBooks();
        string RefreshBookList();
        string DownloadMissing();
        string DownloadBook(string bookID);
        string ExtractBook(string bookID);
    }
    public class BookFunnelService : IBookFunnelService
    {
        private static readonly DatabaseContext _dbContext = new DatabaseContext();

        private static Task _refreshTask;

        private static Task _downloadTask;
        private static List<string> _downloadQueue = new List<string>();

        private static Task _extractTask;
        private static List<string> _extractQueue = new List<string>();

        public List<BookFunnel> GetBooks()
        {
            return _dbContext.BookFunnelItems.OrderByDescending(b => b.DateAdded).ToList();
        }
        public string RefreshBookList()
        {
            return RefreshBookListTask().Result;
        }
        public string DownloadMissing()
        {
            _downloadQueue.AddRange(_dbContext.BookFunnelItems.Where(b => !b.Downloaded).Select(b => b.ID).ToList());
            _extractQueue.AddRange(_dbContext.BookFunnelItems.Where(b => b.Downloaded && !b.Extracted).Select(b => b.ID).ToList());
            return DownloadBookTask().Result;
        }
        public string DownloadBook(string bookID)
        {
            return DownloadBookTask(bookID).Result;
        }
        public string ExtractBook(string bookID)
        {
            return ExtractBookTask(bookID).Result;
        }
        public Task<string> RefreshBookListTask()
        {
            if (_refreshTask != null && (_refreshTask.Status == TaskStatus.Running || _refreshTask.Status == TaskStatus.WaitingToRun || _refreshTask.Status == TaskStatus.WaitingForActivation))
            {
                return Task.FromResult("BookFunnel: Refresh already running");
            }
            else
            {
                _refreshTask = Task.Factory.StartNew(() =>
                {
                    BrowserSession b = new BrowserSession();
                    BookFunnelUtils.Login(b);

                    var booksLive = BookFunnelUtils.GetBookList(b);

                    var booksToAdd = booksLive.Where(b => !_dbContext.BookFunnelItems.Select(i => i.ID).Contains(b.ID));
                    var booksToRemove = _dbContext.BookFunnelItems.Where(b => !booksLive.Select(i => i.ID).Contains(b.ID));

                    _dbContext.BookFunnelItems.AddRange(booksToAdd);
                    _dbContext.BookFunnelItems.RemoveRange(booksToRemove);

                    _dbContext.SaveChanges();
                });

                _refreshTask.Wait();

                return Task.FromResult("BookFunnel: Refresh complete");
            }
        }
        public Task<string> DownloadBookTask(string bookID = null)
        {
            if (bookID != null && !_downloadQueue.Contains(bookID)) _downloadQueue.Add(bookID);
            if (_downloadTask != null && (_downloadTask.Status == TaskStatus.Running || _downloadTask.Status == TaskStatus.WaitingToRun || _downloadTask.Status == TaskStatus.WaitingForActivation))
            {
                return Task.FromResult("BookFunnel: Download already running. Request added to queue");
            }
            else
            {
                if (_downloadQueue.Count() == 0)
                {
                    return Task.FromResult("BookFunnel: Queue Empty");
                }
                else
                {
                    var currentBookID = _downloadQueue.First();

                    _downloadTask = Task.Factory.StartNew(() =>
                    {
                        BrowserSession b = new BrowserSession();
                        BookFunnelUtils.Login(b);

                        var book = _dbContext.BookFunnelItems.First(b => b.ID == currentBookID);
                        string downloadURL = BookFunnelUtils.GetBookDownloadPage(b, book.GetDownloadURL());
                        string zipPath = Path.Combine(FileUtils.GetDownloadPath(), string.Concat(book.Author.Split(Path.GetInvalidFileNameChars())) + " - " + string.Concat(book.Title.Split(Path.GetInvalidFileNameChars())) + ".zip");
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFile(downloadURL, zipPath);
                        }
                        book.Downloaded = true;
                        book.ZipPath = zipPath;
                        _dbContext.SaveChanges();

                    });

                    _downloadTask.Wait();

                    ExtractBook(currentBookID);
                    _downloadQueue.Remove(currentBookID);

                    DownloadBookTask();

                    return Task.FromResult("BookFunnel: Refresh complete");
                }
            }
        }
        public Task<string> ExtractBookTask(string bookID = null)
        {
            if (bookID != null && !_extractQueue.Contains(bookID)) _extractQueue.Add(bookID);
            if (_extractTask != null && (_extractTask.Status == TaskStatus.Running || _extractTask.Status == TaskStatus.WaitingToRun || _extractTask.Status == TaskStatus.WaitingForActivation))
            {
                return Task.FromResult("BookFunnel: Download already running. Request added to queue");
            }
            else
            {
                if (_extractQueue.Count() == 0)
                {
                    return Task.FromResult("BookFunnel: Queue Empty");
                }
                else
                {
                    var currentBookID = _extractQueue.First();

                    _extractTask = Task.Factory.StartNew(() =>
                    {
                        BrowserSession b = new BrowserSession();
                        BookFunnelUtils.Login(b);

                        var book = _dbContext.BookFunnelItems.First(b => b.ID == currentBookID);

                        var itemPath = Path.Combine(FileUtils.GetMediaPath(), string.Concat(book.Author.Split(Path.GetInvalidFileNameChars())), string.Concat(book.Title.Split(Path.GetInvalidFileNameChars())));
                        Directory.CreateDirectory(itemPath);
                        ZipFile.ExtractToDirectory(book.ZipPath, itemPath);
                        //File.Delete(zipPath);

                        book.Extracted = true;
                        book.ExtractedPath = itemPath;
                        _dbContext.SaveChanges();
                    });

                    _extractTask.Wait();

                    _extractQueue.Remove(currentBookID);

                    ExtractBookTask();

                    return Task.FromResult("BookFunnel: Extract complete");
                }
            }
        }
    }
}
