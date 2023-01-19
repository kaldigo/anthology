using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Plugins.Models
{
    public class Metadata
    {
        public string Title { get; set; }
        public string? Subtitle { get; set; }
        public List<string> Authors { get; set; } = new List<string>();
        public List<string> Narrators { get; set; } = new List<string>();
        public List<MetadataSeries> Series { get; set; } = new List<MetadataSeries>();
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public DateTime? PublishDate { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public string? Language { get; set; }
        public bool IsExplicit { get; set; } = false;
        public List<string> Covers { get; set; } = new List<string>();
        public object this[string PropertyName]
        {
            get
            {
                Type myType = typeof(Metadata);
                System.Reflection.PropertyInfo pi = myType.GetProperty(PropertyName);
                if (pi == null) return null;
                return pi.GetValue(this, null); //not indexed property!
            }
            set
            {
                Type myType = typeof(Metadata);
                System.Reflection.PropertyInfo pi = myType.GetProperty(PropertyName);
                pi.SetValue(this, value, null); //not indexed property!
            }
        }
        public class MetadataSeries
        {
            public string Name { get; set; }
            public string VolumeNumber { get; set; }
            public MetadataSeries() { }
            public MetadataSeries(string name, string volumeNumber)
            {
                Name = name;
                VolumeNumber = volumeNumber;

            }
        }
    }
    public class MetadataSearchResult
    {
        public string Key { get; set; }
        public string Identifier { get; set; }
        public Metadata Metadata { get; set; }
    }
}
