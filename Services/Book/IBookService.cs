using Anthology.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Anthology.Services.MetadataService;

namespace Anthology.Services
{
    public interface IBookService
    {
        Task<List<ApiMetadata>> Search(Dictionary<string, string> searchQuery);
        List<Book> GetBooks();
        Task<List<Book>> GetBooksAsync();
        List<Book> GetBooksWithStatus();
        Task<List<Book>> GetBooksWithStatusAsync();
        void SaveBooks(List<Book> book, bool updateMetadata = false, bool metadataRefreshed = false);
        void SaveBook(Book book, bool updateMetadata = false, bool metadataRefreshed = false);
        void DeleteBook(string isbn);
    }
}
