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

        public BookService(DatabaseContext context, IMetadataService metadataService)
        {
            _context = context;
            _metadataService = metadataService;
        }

        public List<Book> GetBooks()
        {
            return _context.Books.ToList();
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

        public void SaveBook(Book book)
        {
            var updateMetadata = false;
            if (book.ISBN == null)
            {
                book.ISBN = Book.GenerateId(_context);
                _context.Books.Add(book);
                updateMetadata = true;
            }
            else
            {
                var dbBook = _context.Books.SingleOrDefault(x => x.ISBN == book.ISBN);
                if (dbBook.Identifiers != book.Identifiers)
                    updateMetadata = true;
                _context.Books.Update(book);
            }
            _context.SaveChanges();
            if (updateMetadata) _metadataService.RefreshBookMetadata(book);
        }

        public void DeleteBook(string isbn)
        {
            var book = _context.Books.FirstOrDefault(x => x.ISBN == isbn);
            if (book != null)
            {
                _context.Books.Remove(book);
                _context.SaveChanges();
            }
        }
    }
}
