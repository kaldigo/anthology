using Anthology.Data;
using Anthology.Plugins.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Services
{
    public interface IMetadataService
    {
        Task<List<dynamic>> Search(Dictionary<string, string> searchQuery);
        Task<dynamic> GetApiMetadata(Book book);
        Task<Metadata> GetMetadata(Book book, bool forceRefresh);
        Task RefreshBookMetadata(Book dbBook);
    }
}
