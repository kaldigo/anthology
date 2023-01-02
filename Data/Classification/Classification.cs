using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data
{
    public class Classification
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; }
        public string Name { get; set; }
        public ClassificationType Type { get; set; }
        public virtual List<ClassificationAlias> Aliases { get; set; } = new List<ClassificationAlias>();

        public Classification() { }

        public enum ClassificationType
        {
            Genre,
            Tag,
            Hidden
        }
    }
}
