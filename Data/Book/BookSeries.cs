using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data
{
    public class BookSeries
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string Sequence { get; set; }
        public virtual Book Book { get; set; }
        public virtual Series Series { get; set; }
        public BookSeries() { }
        public BookSeries(Series series, string sequence)
        {
            this.Series = series;
            this.Sequence = sequence;
        }
    }
}
