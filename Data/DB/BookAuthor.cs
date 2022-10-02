using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data.DB
{
    public class BookAuthor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string Name { get; set; }
        public virtual List<Book> Books { get; set; }
        public BookAuthor() {}
        public BookAuthor(string name)
        {
            this.Name = name;
        }
    }
}
