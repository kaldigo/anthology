using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Plugins.Models
{
    public class Import
    {
        public string Key { get; set; }
        public string Identifier { get; set; }
        public string Title { get; set; }
        public List<string> Authors { get; set; }
    }
}
