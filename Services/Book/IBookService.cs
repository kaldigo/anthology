using Anthology.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Services
{
    public interface IBookService
    {
        Task<List<dynamic>> Search(Dictionary<string, string> searchQuery);
        List<Book> GetBooks();
        Task<List<Book>> GetBooksAsync();
        List<Book> GetBooks(string title);
        Book GetBookByISBN(string isbn);
        void SaveBook(Book book, bool updateMetadata = false);
        void DeleteBook(string isbn);
    }
}
