using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data.DB
{
    public class BookGenre
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string Name { get; set; }
        public virtual List<Book> Books { get; set; }
        public BookGenre()
        {
        }
        public BookGenre(string name)
        {
            this.Name = name;
        }
    }
}
