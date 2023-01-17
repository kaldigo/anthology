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
        Task<dynamic> GetApiMetadata(Book book);
        Task<List<MetadataSearchResult>> SearchMetadata(Plugin plugin, string title, string author);
        Task<Metadata> GetMetadata(Book book, bool forceRefresh = false);
        Task RefreshBookMetadata(Book dbBook);
    }
}
