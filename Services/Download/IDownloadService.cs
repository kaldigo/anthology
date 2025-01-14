using Anthology.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anthology.Plugins.Models;
using Anthology.Plugins;

namespace Anthology.Services
{
    public interface IDownloadService
    {
        public Task<List<Download>> GetDownloadList();
        public Task DownloadBook(Download download);
        event Action<DownloadProgressEventArgs> OnDownloadProgressChanged;
        public List<Download> ActiveDownloads { get; }
    }
}
