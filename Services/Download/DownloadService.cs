using Anthology.Data;
using Anthology.Plugins;
using Anthology.Plugins.Models;

namespace Anthology.Services
{
    public class DownloadService : IDownloadService
    {
        IPluginsService _pluginsService;
        ISettingsService _settingsService;

        public DownloadService(IPluginsService pluginsService, ISettingsService settingsService)
        {
            _pluginsService = pluginsService;
            _settingsService = settingsService;
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
            var plugin = _pluginsService.GetPluginList().FirstOrDefault(p => p.Type == Plugin.PluginType.Download && p.Identifier == download.Key);
            if (plugin == null) throw new NullReferenceException();

            var downloadInstance = Activator.CreateInstance(plugin.ClassType) as IDownloadSource;
            var downloadSettings = _settingsService.GetSettings().PluginSettings.Where(s => s.PluginName == plugin.Name).SelectMany(s => s.Settings).ToDictionary(s => s.Key, s => s.Value);

            downloadInstance.DownloadBook(download, Utils.FileUtils.GetMediaPath(), downloadSettings);
        }
    }
}
