using Anthology.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Task<List<dynamic>> Search(Dictionary<string, string> searchQuery)
        {
            if (searchQuery["title"].Length > 4 && searchQuery["title"].Substring(0, 4) == "ANTH") searchQuery["isbn"] = searchQuery["title"];

            if (searchQuery["isbn"] != null && searchQuery["isbn"].Substring(0, 4) == "ANTH")
            {
                var book = GetBookByISBN(searchQuery["isbn"]);
                if (book != null)
                {
                    var searchISBN = _metadataService.GetMetadata(book).Result;

                    var searchISBNList = new List<dynamic>();
                    searchISBNList.Add(searchISBN);
                    return Task.FromResult(searchISBNList);

                }
            }

            var books = GetBooks(searchQuery["title"]).Select(b => (dynamic)(_metadataService.GetApiMetadata(b))).ToList();
            _context.SaveChanges();
            return Task.FromResult(books);
        }

        public List<Book> GetBooks()
        {
            return _libraryService.SetBookLibraryStatus(_context.Books.ToList()).Result;
        }

        public async Task<List<Book>> GetBooksAsync()
        {
            Task.Delay(0);
            return GetBooks();
        }

        public List<Book> GetBooks(string title)
        {
            var books = _context.Books.Where(b => b.Title.Contains(title) || title.Contains(b.Title)).ToList();
            return books;
        }

        public Book GetBookByISBN(string isbn)
        {
            var book = _context.Books.SingleOrDefault(x => x.ISBN == isbn);
            return book;
        }

        public void SaveBook(Book book, bool updateMetadata = false)
        {
            var metadataIdentifiers = _pluginsService.GetPluginList().Where(p => p.Type == Plugin.PluginType.Metadata)
                .Select(p => p.Identifier);

            bool IdentifiersChanged = false;
            book.Identifiers.RemoveAll(i => book.Identifiers.Where(a => (string.IsNullOrWhiteSpace(a.Value) && a.Exists)).Select(r => r.Key).Contains(i.Key));
            if (book.ISBN == null)
            {
                book.ISBN = Book.GenerateId(_context);
                _context.Books.Add(book);
                if(book.Identifiers.Any(i => metadataIdentifiers.Contains(i.Key))) updateMetadata = true;
            }
            else
            {
                var dbBook = _context.Books.SingleOrDefault(x => x.ISBN == book.ISBN);
                _context.Books.Update(book);
            }
            _context.SaveChanges();
            if (updateMetadata)
            {
                _metadataService.RefreshBookMetadata(book);
                _context.SaveChanges();
            }
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
