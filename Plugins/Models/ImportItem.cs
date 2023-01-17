using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Plugins.Models
{
    public class ImportItem
    {
        public string Key { get; set; }
        public string Identifier { get; set; }
        public List<KeyValuePair<string,string>> Identifiers { get; set; }
        public Metadata Metadata { get; set; }
    }
}
