using Anthology.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Anthology.Services.MetadataService;

namespace Anthology.Services
{
    public class BookService : IBookService
    {
        DatabaseContext _context;
        IMetadataService _metadataService;
        ILibraryService _libraryService;
        IPluginsService _pluginsService;

        public BookService(DatabaseContext context, IMetadataService metadataService, ILibraryService libraryService, IPluginsService pluginsService)
        {
            _context = context;
            _metadataService = metadataService;
            _libraryService = libraryService;
            _pluginsService = pluginsService;
        }

        public Task<List<ApiMetadata>> Search(Dictionary<string, string> searchQuery)
        {
            if (searchQuery["title"].Length > 4 && searchQuery["title"].Substring(0, 4) == "ANTH") searchQuery["isbn"] = searchQuery["title"];

            if (searchQuery.ContainsKey("isbn") && searchQuery["isbn"].Substring(0, 4) == "ANTH")
            {
                var book = _context.Books.SingleOrDefault(x => x.ISBN == searchQuery["isbn"]); ;
                if (book != null) return Task.FromResult(new List<ApiMetadata>(){ _metadataService.GetApiMetadata(book, searchQuery["host"]).Result });
            }

            var books = _context.Books.ToList();

            books = books.Where(b => Utils.StringUtils.CompareStrings(searchQuery["title"],
                string.IsNullOrWhiteSpace(b.Title) ? b.BookMetadata.Title : b.Title)).ToList();
            if (searchQuery.ContainsKey("author"))
                books = books.Where(b =>
                    Utils.StringUtils.CompareStrings(searchQuery["author"],
                        b.Authors.Count == 0
                            ? string.Join(", ", b.BookMetadata.Authors)
                            : string.Join(", ", b.Authors.Select(a => a.Name)))).ToList();

            var searchResults = books.Select(b => _metadataService.GetApiMetadata(b, searchQuery["host"]).Result).ToList();
            _context.SaveChanges();
            if (_metadataService.IsPendingRefresh()) _metadataService.RefreshMetadataCache();
            return Task.FromResult(searchResults);
        }

        public List<Book> GetBooks()
        {
            return _context.Books.ToList();
        }

        public async Task<List<Book>> GetBooksAsync()
        {
            Task.Delay(0);
            return GetBooks();
        }

        public async Task<List<Book>> GetBooksWithStatusAsync()
        {
            Task.Delay(0);
            return GetBooksWithStatus();
        }

        public List<Book> GetBooksWithStatus()
        {
            return _libraryService.SetBookLibraryStatus(_context.Books.ToList()).Result;
        }

        public List<Book> GetBooks(string title)
        {
            var books = _context.Books.Where(b => Utils.StringUtils.CompareStrings(title, b.Title)).ToList();
            return books;
        }

        public Book GetBookByISBN(string isbn)
        {
            var book = _context.Books.SingleOrDefault(x => x.ISBN == isbn);
            return book;
        }

        public void SaveBook(Book book, bool updateMetadata = false, bool metadataRefreshed = false)
        {
            // Synchronous portion
            var metadataIdentifiers = _pluginsService.GetPluginList()
                .Where(p => p.Type == Plugin.PluginType.Metadata)
                .Select(p => p.Identifier);

            book.Identifiers.RemoveAll(i => book.Identifiers
                .Where(a => string.IsNullOrWhiteSpace(a.Value) && a.Exists)
                .Select(r => r.Key)
                .Contains(i.Key));

            var uniqueIdentifiers = book.Identifiers
                .GroupBy(i => i.Key)
                .Select(g =>
                {
                    var nonEmpty = g.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Value));
                    return nonEmpty ?? g.First();
                })
                .ToList();

            book.Identifiers = uniqueIdentifiers;

            if (book.ISBN == null)
            {
                book.ISBN = Book.GenerateId(_context);
                _context.Books.Add(book);
                if (book.Identifiers.Any(i => metadataIdentifiers.Contains(i.Key)))
                {
                    updateMetadata = true;
                }
            }
            else
            {
                var dbBook = _context.Books.SingleOrDefault(x => x.ISBN == book.ISBN);
                _context.Books.Update(book);
            }

            _context.SaveChanges(); // Blocking save for critical operations

            // Asynchronous portion
            Task.Run(async () =>
            {
                if (updateMetadata)
                {
                    _metadataService.RefreshBookMetadata(book);
                    await _context.SaveChangesAsync();
                }
                if (metadataRefreshed)
                {
                    _metadataService.RefreshMetadataCache();
                }
            });
        }


        public void DeleteBook(string isbn)
        {
            var book = _context.Books.FirstOrDefault(x => x.ISBN == isbn);
            if (book != null)
            {
                foreach (var image in book.BookCovers) File.Delete(image.GetPath());
                foreach (var image in book.AudiobookCovers) File.Delete(image.GetPath());
                foreach (var image in book.Images) File.Delete(image.GetPath());
                _context.Books.Remove(book);
                _context.SaveChanges();
                ProcessDirectory(Utils.FileUtils.GetMediaPath());
            }
        }

        private static void ProcessDirectory(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                ProcessDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }
    }
}
