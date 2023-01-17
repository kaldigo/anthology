using Anthology.Data;
using Anthology.Plugins;
using Anthology.Plugins.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Services
{
    public class ImportService : IImportService
    {
        IPluginsService _pluginsService;
        ISettingsService _settingsService;
        IBookService _bookService;

        public ImportService(IPluginsService pluginsService, ISettingsService settingsService, IBookService bookService)
        {
            _pluginsService = pluginsService;
            _settingsService = settingsService;
            _bookService = bookService;
        }

        public Task<List<ImportItem>> GetImportList()
        {
            var import = new List<ImportItem>();

            var bookList = _bookService.GetBooks();

            var importPlugins = _pluginsService.GetPluginList().Where(p => p.Type == Plugin.PluginType.Import).ToList();

            foreach (var plugin in importPlugins)
            {
                    var importInstance = Activator.CreateInstance(plugin.ClassType) as IImportSource;
                    var importSettings = _settingsService.GetSettings().PluginSettings.Where(s => s.PluginName == plugin.Name).SelectMany(s => s.Settings).ToDictionary(s => s.Key, s => s.Value);
                    var importList = importInstance.GetImportItems(importSettings);

                    var matchingISBNBooks = importList.Where(i =>
                        i.Identifiers.Any(id => id.Key == "ISBN") && bookList.Any(b =>
                            b.ISBN == i.Identifiers.First(id => id.Key == "ISBN").Value));

                    foreach (var i in matchingISBNBooks)
                    {
                        var book = bookList.First(b => b.ISBN == i.Identifiers.First(id => id.Key == "ISBN").Value);
                        if(book.Identifiers.Any(id => id.Key == i.Key)) book.Identifiers.First(id => id.Key == i.Key).Value = i.Identifier;
                        else book.Identifiers.Add(new BookIdentifier(i.Key, i.Identifier));
                        _bookService.SaveBook(book);
                    }

                    import.AddRange(importList.Where(i => !(bookList.SelectMany(b => b.Identifiers)
                        .Where(i => i.Key == importInstance.IdentifierKey).Select(i => i.Value).Contains(i.Identifier))));
            }

            return Task.FromResult(import);
        }
    }
}
