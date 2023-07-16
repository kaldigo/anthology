using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data
{
    public class Series
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string Name { get; set; }
        public virtual List<BookSeries> BooksJoin { get; set; } = new List<BookSeries>();
        [NotMapped]
        public List<Book> Books
        {
            get
            {
                return BooksJoin.Select(b => b.Book).ToList();
            }
        }
        public virtual List<SeriesAlias> Aliases { get; set; } = new List<SeriesAlias>();
        public virtual List<Classification> Classifications { get; set; } = new List<Classification>();
        [NotMapped]
        public List<Classification> BookClassificationsRaw
        {
            get
            {
                var genres = Books.Where(b => b != null).SelectMany(b => b.BookMetadata.Genres).GroupBy(c => c).Where(c => c.Count() > 1).Select(c => new Classification() { Name = c.Key, Type = Classification.ClassificationType.Genre });
                var tags = Books.Where(b => b != null).SelectMany(b => b.BookMetadata.Tags).GroupBy(c => c).Where(c => c.Count() > 1).Select(c => new Classification() { Name = c.Key, Type = Classification.ClassificationType.Tag });
                return genres.Union(tags).ToList();
            }
        }
        [NotMapped]
        public List<Classification> BookClassifications { get; set; } = new List<Classification>();
        public Series() { }
        public Series(string name)
        {
            this.Name = name;
        }
    }
}
