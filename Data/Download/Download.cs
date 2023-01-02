using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Anthology.Data
{
    public class Download
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string Key { get; set; }
        public string Identifier { get; set; }
        public string? Title { get; set; }
        public virtual List<DownloadAuthor> Author { get; set; } = new List<DownloadAuthor>();
        public string? ImageURL { get; set; }
        public DateTime DateAdded { get; set; }
        public virtual Book? Book { get; set; }

    }
}
