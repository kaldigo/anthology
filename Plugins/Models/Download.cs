using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Plugins.Models
{
    public class Download
    {
        public string Key { get; set; }
        public string Identifier { get; set; }
        public string Title { get; set; }
        public List<string> Author { get; set; }
        public string ImageURL { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
