using Anthology.Data;
using Anthology.Plugins;

namespace Anthology.Services
{
    public class LibraryService : ILibraryService
    {
        IPluginsService _pluginsService;
        ISettingsService _settingsService;

        public LibraryService(IPluginsService pluginsService, ISettingsService settingsService)
        {
            _pluginsService = pluginsService;
            _settingsService = settingsService;
        }

        public Task<Book> SetBookLibraryStatus(Book book)
        {
            var libraryPlugins = _pluginsService.GetPluginList().Where(p => p.Type == Plugin.PluginType.Import).ToList();

            foreach (var plugin in libraryPlugins)
            {
                var libraryInstance = Activator.CreateInstance(plugin.ClassType) as ILibrarySource;
                var librarySettings = _settingsService.GetSettings().PluginSettings.Where(s => s.PluginName == plugin.Name).SelectMany(s => s.Settings).ToDictionary(s => s.Key, s => s.Value);

                book.ExistsInLibrary = libraryInstance.IsBookInLibrary(book.ISBN, librarySettings);
            }

            return Task.FromResult(book);
        }

        public Task<List<Book>> SetBookLibraryStatus(List<Book> books)
        {
            var libraryPlugins = _pluginsService.GetPluginList().Where(p => p.Type == Plugin.PluginType.Library).ToList();

            foreach (var plugin in libraryPlugins)
            {
                var libraryInstance = Activator.CreateInstance(plugin.ClassType) as ILibrarySource;
                var librarySettings = _settingsService.GetSettings().PluginSettings.Where(s => s.PluginName == plugin.Name).SelectMany(s => s.Settings).ToDictionary(s => s.Key, s => s.Value);

                books = books.Select(b =>
                {
                    b.ExistsInLibrary = libraryInstance.IsBookInLibrary(b.ISBN, librarySettings);
                    return b;
                }).ToList();
            }

            return Task.FromResult(books);
        }
    }
}
