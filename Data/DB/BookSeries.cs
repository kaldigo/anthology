using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data.DB
{
    public class BookSeries
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string Name { get; set; }
        public float? Sequence { get; set; }
        public virtual List<Book> Books { get; set; }
        public BookSeries() { }
        public BookSeries(string name, float sequence)
        {
            this.Name = name;
            this.Sequence = sequence;
        }
    }
}
