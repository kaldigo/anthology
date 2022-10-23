using Anthology.Data.DB;
using Anthology.Utils;
using Microsoft.EntityFrameworkCore;

namespace Anthology.Services
{
    public interface IBookService
    {
        List<Book> GetBooks();
        Book GetBookByISBN(string isbn);
        void SaveBook(Book book);
        void DeleteBook(string isbn);
        void GetMissingBookMetadata();
        void RefreshMetadata(Book book);
    }
    public class BookService: IBookService
    {
        private static readonly DatabaseContext _dbContext = new DatabaseContext();
        public List<Book> GetBooks()
        {
            return _dbContext.Books.ToList();
        }
        public Book GetBookByISBN(string isbn)
        {
            var book = _dbContext.Books.SingleOrDefault(x => x.ISBN == isbn);
            return book;
        }
        public void SaveBook(Book book)
        {
            var updateMetadata = false;
            if (book.ISBN == null)
            {
                book.ISBN = Book.GenerateId(_dbContext);
                _dbContext.Books.Add(book);
                updateMetadata = true;
            }
            else
            {
                var dbBook = _dbContext.Books.SingleOrDefault(x => x.ISBN == book.ISBN);
                if(
                    dbBook.GRID != book.GRID || 
                    dbBook.ASIN != book.ASIN ||
                    dbBook.AGID != book.AGID ) 
                    updateMetadata = true;
                _dbContext.Books.Update(book);
            }
            _dbContext.SaveChanges();
            if(updateMetadata) MetadataService.RefreshBookMetadata(book, _dbContext);
        }
        public void DeleteBook(string isbn)
        {
            var book = _dbContext.Books.FirstOrDefault(x => x.ISBN == isbn);
            if (book != null)
            {
                _dbContext.Books.Remove(book);
                _dbContext.SaveChanges();
            }
        }
        public void GetMissingBookMetadata()
        {
            foreach (var book in _dbContext.Books.Where(b => b.DateFetchedMetadata == null || b.DateFetchedMetadata > DateTime.Now.AddDays(30)))
            {
                MetadataService.RefreshBookMetadata(book, _dbContext);
            }
        }
        public void RefreshMetadata(Book book)
        {
            MetadataService.RefreshBookMetadata(book, _dbContext);
        }
    }
}
