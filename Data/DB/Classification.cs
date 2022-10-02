using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data.DB
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
            Tag
        }
        public static List<Classification> GetClassification(List<string> rawData, DatabaseContext? context = null)
        {
            if (context == null) context = new DatabaseContext();

            var list = new List<Classification>();

            foreach (var item in rawData)
            {
                var matched = context.Classifications.Where(c => c.Name.ToLower() == item.ToLower() || c.Aliases.Any(a => a.Name.ToLower() == item.ToLower()));
                if (matched.Any()) list.AddRange(matched);
                else list.Add(new Classification() { Name = item, Type = ClassificationType.Genre });
            }

            return list;
        }
    }
}
