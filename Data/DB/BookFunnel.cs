using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data.DB
{
    public class BookFunnel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string ID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ImageURL { get; set; }
        public DateTime DateAdded { get; set; }
        public bool Downloaded { get; set; } = false;
        public bool Extracted { get; set; } = false;
        public string? ZipPath { get; set; }
        public string? ExtractedPath { get; set; }

        public string GetDownloadURL()
        {
            return "https://my.bookfunnel.com/" + ID + "/download";
        }
        public enum ImageSize
        {
            Small,
            Medium,
            Large
        }
    }
}
