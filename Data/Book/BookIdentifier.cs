using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Data
{
    public class BookIdentifier
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string Key { get; set; }
        public string? Value { get; set; }
        public bool Exists { get; set; } = true;
        public virtual Book Book { get; set; }
        public BookIdentifier() { }
        public BookIdentifier(string key, string value, bool exists = true)
        {
            this.Key = key;
            this.Value = value;
            this.Exists = exists;
        }

        public BookIdentifier Clone()
        {
            return new BookIdentifier(this.Key, this.Value, this.Exists);
        }
    }
}
