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
        List<Book> GetBooks();
        List<Book> GetBooks(string title);
        Book GetBookByISBN(string isbn);
        void SaveBook(Book book);
        void DeleteBook(string isbn);
    }
}
