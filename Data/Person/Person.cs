using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data
{
    public class Person
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string Name { get; set; }
        public virtual List<PersonAlias> Aliases { get; set; } = new List<PersonAlias>();

        [InverseProperty("Authors")]
        public virtual List<Book> BooksAuthored { get; set; } = new List<Book>();

        [InverseProperty("Narrators")]
        public virtual List<Book> BooksNarrated { get; set; } = new List<Book>();
        public virtual List<PersonImage> Images { get; set; } = new List<PersonImage>();
        public Person() {}
        public Person(string name)
        {
            this.Name = name;
        }
    }
}
