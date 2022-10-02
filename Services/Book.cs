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
    }
    public class BookService: IBookService
    {
        private static readonly DatabaseContext _dbContext = new DatabaseContext();
        public static Task<List<Book>> FetchBooks()
        {
            var context = new DatabaseContext();
            var bookList = context.Books.ToList();
            return Task.FromResult(bookList);
        }
        public static Task<List<Book>> FetchBooksToMatch(string source, int numberToTake, int numberToSkip)
        {
            var context = new DatabaseContext();
            List<Book>? bookList;
            switch (source)
            {
                case "Audible":
                    bookList = context.Books.Where(b => b.AudibleExists && (b.ASIN == null || string.IsNullOrEmpty(b.ASIN))).ToList();
                    break;
                case "AudiobookGuild":
                    bookList = context.Books.Where(b => b.AudiobookGuildExists && (b.AGID == null || string.IsNullOrEmpty(b.AGID)) && b.Authors.Any(a => AudiobookGuild.AGAuthors.Contains(a.Name))).ToList();
                    break;
                default:
                    bookList = context.Books.ToList();
                    break;
            }

            bookList = bookList
                .Skip(numberToSkip)
                .Take(numberToTake)
                .ToList();

            return Task.FromResult(bookList);
        }
        public static async Task<List<Book>> Get()
        {
            var books = await _dbContext.Books.ToListAsync();
            return books;
        }

        public static async Task<Book> Get(string isbn)
        {
            var book = await _dbContext.Books.FirstOrDefaultAsync(a => a.ISBN == isbn);
            return book;
        }

        public static async Task<string> Post(Book book)
        {
            _dbContext.Add(book);
            await _dbContext.SaveChangesAsync();
            return book.ISBN;
        }

        public static async Task Put(Book book)
        {
            _dbContext.Entry(book).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public static async Task Delete(string isbn)
        {
            var book = new Book { ISBN = isbn };
            _dbContext.Remove(book);
            await _dbContext.SaveChangesAsync();
        }
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
            if (book.ISBN == null)
            {
                book.ISBN = Book.GenerateId(_dbContext);
                _dbContext.Books.Add(book);
            }
            else _dbContext.Books.Update(book);
            _dbContext.SaveChanges();
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
    }
}
