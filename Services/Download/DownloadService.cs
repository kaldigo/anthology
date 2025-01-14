using Anthology.Data;
using Anthology.Plugins;
using Anthology.Plugins.Models;

namespace Anthology.Services
{
    public class DownloadService : IDownloadService
    {
        IPluginsService _pluginsService;
        ISettingsService _settingsService;
        IBookService _bookService;

        public event Action<DownloadProgressEventArgs> OnDownloadProgressChanged;

        private readonly List<Download> _activeDownloads = new List<Download>();
        public List<Download> ActiveDownloads => _activeDownloads;

        public DownloadService(IPluginsService pluginsService, ISettingsService settingsService, IBookService bookService)
        {
            _pluginsService = pluginsService;
            _settingsService = settingsService;
            _bookService = bookService;
        }

        public Task<List<Download>> GetDownloadList()
        {
            var downloadList = new List<Download>();
            var downloadPlugins = _pluginsService.GetPluginList().Where(p => p.Type == Plugin.PluginType.Download).ToList();

            foreach (var plugin in downloadPlugins)
            {
                var downloadInstance = Activator.CreateInstance(plugin.ClassType) as IDownloadSource;
                var downloadSettings = _settingsService.GetSettings().PluginSettings.Where(s => s.PluginName == plugin.Name).SelectMany(s => s.Settings).ToDictionary(s => s.Key, s => s.Value);

                downloadList.AddRange(downloadInstance.GetDownloadList(downloadSettings));
            }

            return Task.FromResult(downloadList.OrderByDescending(d => d.DateAdded).ToList());
        }

        public async Task DownloadBook(Download download)
        {
            ActiveDownloads.Add(download);
            try
            {
                var book = _bookService.GetBooks().FirstOrDefault(b => b.Identifiers.Any(i => i.Key == download.Key && i.Value == download.Identifier));
                if (book != null)
                {
                    download.Title = string.IsNullOrWhiteSpace(book.Title) ? book.BookMetadata.Title : book.Title;
                    download.Author = book.Authors.Count == 0 ? book.BookMetadata.Authors : book.Authors.Select(a => a.Name).ToList();
                }

                var plugin = _pluginsService.GetPluginList().FirstOrDefault(p => p.Type == Plugin.PluginType.Download && p.Identifier == download.Key);
                if (plugin == null) throw new NullReferenceException();

                var downloadInstance = Activator.CreateInstance(plugin.ClassType) as IDownloadSource;
                var downloadSettings = _settingsService.GetSettings().PluginSettings.Where(s => s.PluginName == plugin.Name).SelectMany(s => s.Settings).ToDictionary(s => s.Key, s => s.Value);

                // Subscribe once
                downloadInstance.OnDownloadProgressChanged += (args) =>
                {
                    // Relay the event back to anyone listening to *this* service
                    OnDownloadProgressChanged?.Invoke(args);
                };

                downloadInstance.DownloadBook(download, Utils.FileUtils.GetDownloadPath(), downloadSettings);
                ActiveDownloads.Remove(download);
            }
            catch
            {
                ActiveDownloads.Remove(download);
                throw;
            }
        }
    }
}