using Anthology.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Services
{
    public interface ILibraryService
    {
        public Task<Book> SetBookLibraryStatus(Book book);
        public Task<List<Book>> SetBookLibraryStatus(List<Book> books);
    }
}
