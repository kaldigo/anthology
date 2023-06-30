using Anthology.Data;
using Anthology.Plugins.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Anthology.Services.MetadataService;

namespace Anthology.Services
{
    public interface IMetadataService
    {
        bool IsPendingRefresh();
        Task<ApiMetadata> GetApiMetadata(Book book, string host);
        Task<List<MetadataSearchResult>> SearchMetadata(Plugin plugin, string title, string author);
        Task<Metadata> GetMetadata(Book book, bool forceRefresh = false);
        Task RefreshMetadataCache();
        Task RefreshBookMetadata(Book dbBook);
    }
}
