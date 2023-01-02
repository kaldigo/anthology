using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anthology.Plugins.Models;

namespace Anthology.Plugins
{
    public interface IMetadataSource
    {
        string Name { get; }
        string IdentifierKey { get; }
        List<string> Settings { get; }
        List<Metadata> Search(Dictionary<string,string> settings, string title, string author = null);
        Metadata GetMetadata(string identifier, Dictionary<string,string> settings);

    }
}
