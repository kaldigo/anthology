using Anthology.Plugins.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Plugins
{
    public interface IImportSource
    {
        string Name { get; }
        string IdentifierKey { get; }
        List<string> Settings { get; }
        List<ImportItem> GetImportItems(Dictionary<string, string> settings);
    }
}
