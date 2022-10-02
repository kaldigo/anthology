using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data.DB
{
    public class BookNarrator
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string Name { get; set; }
        public virtual List<Book> Books { get; set; }
        public BookNarrator()
        {
        }
        public BookNarrator(string name)
        {
            this.Name = name;
        }
    }
}
