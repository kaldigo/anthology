using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data.DB
{
    public class BookTag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string Name { get; set; }
        public virtual List<Book> Books { get; set; }
        public BookTag()
        {
        }
        public BookTag(string name)
        {
            this.Name = name;
        }
    }
}
