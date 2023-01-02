using Anthology.Plugins.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Plugins
{
    public interface IDownloadSource
    {
        string Name { get; }
        string IdentifierKey { get; }
        List<string> Settings { get; }
        List<Download> RefreshList(Dictionary<string,string> settings);
        bool DownloadBook(Download download, Dictionary<string,string> settings);
    }
}
