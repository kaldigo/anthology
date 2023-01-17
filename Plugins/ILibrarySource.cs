using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anthology.Plugins.Models;

namespace Anthology.Plugins
{
    public interface ILibrarySource
    {
        string Name { get; }
        List<string> Settings { get; }
        bool IsBookInLibrary(string isbn, Dictionary<string,string> settings);
    }
}
