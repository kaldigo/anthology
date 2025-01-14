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
        List<Download> GetDownloadList(Dictionary<string,string> settings);
        bool DownloadBook(Download download, string mediaPath, Dictionary<string, string> settings);
        event Action<DownloadProgressEventArgs> OnDownloadProgressChanged;
    }

    public class DownloadProgressEventArgs : EventArgs
    {
        public Download Download { get; set; }         // The current Download object
        public string Status { get; set; }            // e.g. "Downloading", "Extracting", "Complete", "Error"
        public double? PercentComplete { get; set; }   // Optional: if you want numeric progress
    }
}
