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
        Task<ApiMetadata> GetApiMetadata(Book book);
        Task<List<MetadataSearchResult>> SearchMetadata(Plugin plugin, string title, string author);
        Task<Metadata> GetMetadata(Book book, bool forceRefresh = false);
        Task RefreshBookMetadata(Book dbBook);
    }
}
