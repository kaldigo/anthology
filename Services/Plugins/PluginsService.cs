using System.Reflection;
using Anthology.Plugins;

namespace Anthology.Services
{
    public class PluginsService : IPluginsService
    {
        private List<Plugin> _pluginList;
        private List<Plugin> GeneratePluginList()
        {
            var pluginList = new List<Plugin>();
            var metadataPlugins = Assembly.Load("Anthology.Plugins.MetadataSources").GetTypes().Where(p => typeof(IMetadataSource).IsAssignableFrom(p));
            var libraryPlugins = Assembly.Load("Anthology.Plugins.LibrarySources").GetTypes().Where(p => typeof(ILibrarySource).IsAssignableFrom(p));
            var importPlugins = Assembly.Load("Anthology.Plugins.ImportSources").GetTypes().Where(p => typeof(IImportSource).IsAssignableFrom(p));
            var downloadPlugins = Assembly.Load("Anthology.Plugins.DownloadSources").GetTypes().Where(p => typeof(IDownloadSource).IsAssignableFrom(p));

            foreach (var pluginType in metadataPlugins)
            {
                var sourceInstance = Activator.CreateInstance(pluginType) as IMetadataSource;
                pluginList.Add(new Plugin()
                {
                    Type = Plugin.PluginType.Metadata,
                    Name = sourceInstance.Name,
                    Identifier = sourceInstance.IdentifierKey,
                    SettingKeys = sourceInstance.Settings,
                    ClassType = pluginType,
                });
            }

            foreach (var pluginType in libraryPlugins)
            {
                var sourceInstance = Activator.CreateInstance(pluginType) as ILibrarySource;
                pluginList.Add(new Plugin()
                {
                    Type = Plugin.PluginType.Library,
                    Name = sourceInstance.Name,
                    Identifier = "",
                    SettingKeys = sourceInstance.Settings,
                    ClassType = pluginType,
                });
            }

            foreach (var pluginType in importPlugins)
            {
                var sourceInstance = Activator.CreateInstance(pluginType) as IImportSource;
                pluginList.Add(new Plugin()
                {
                    Type = Plugin.PluginType.Import,
                    Name = sourceInstance.Name,
                    Identifier = sourceInstance.IdentifierKey,
                    SettingKeys = sourceInstance.Settings,
                    ClassType = pluginType,
                });
            }

            foreach (var pluginType in downloadPlugins)
            {
                var sourceInstance = Activator.CreateInstance(pluginType) as IDownloadSource;
                pluginList.Add(new Plugin()
                {
                    Type = Plugin.PluginType.Download,
                    Name = sourceInstance.Name,
                    Identifier = sourceInstance.IdentifierKey,
                    SettingKeys = sourceInstance.Settings,
                    ClassType = pluginType,
                });
            }

            return pluginList;
        }

        public List<Plugin> GetPluginList()
        {
            if (_pluginList == null) _pluginList = GeneratePluginList();
            return _pluginList;
        }
    }
    public class Plugin
    {
        public PluginType Type { get; set; }
        public string Name { get; set; }
        public string Identifier { get; set; }
        public List<string> SettingKeys { get; set; }
        public Type ClassType { get; set; }

        public enum PluginType
        {
            Metadata,
            Library,
            Import,
            Download
        }

    }
}
