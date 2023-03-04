using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data
{
    public class ClassificationAlias
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string Name { get; set; }
        public ClassificationAlias() {}

        public ClassificationAlias(string name)
        {
            Name = name;
        }
    }
}
