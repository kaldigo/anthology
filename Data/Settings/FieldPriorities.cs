using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data
{
    public class FieldPriorities
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public virtual List<SourcePriority> Title { get; set; } = new List<SourcePriority>();
        public virtual List<SourcePriority> Subtitle { get; set; } = new List<SourcePriority>();
        public virtual List<SourcePriority> Authors { get; set; } = new List<SourcePriority>();
        public virtual List<SourcePriority> Narrators { get; set; } = new List<SourcePriority>();
        public virtual List<SourcePriority> Series { get; set; } = new List<SourcePriority>();
        public virtual List<SourcePriority> Description { get; set; } = new List<SourcePriority>();
        public virtual List<SourcePriority> Publisher { get; set; } = new List<SourcePriority>();
        public virtual List<SourcePriority> PublishDate { get; set; } = new List<SourcePriority>();
        public bool MergeGenres { get; set; } = true;
        public virtual List<SourcePriority> Genres { get; set; } = new List<SourcePriority>();
        public bool MergeTags { get; set; } = true;
        public virtual List<SourcePriority> Tags { get; set; } = new List<SourcePriority>();
        public virtual List<SourcePriority> Language { get; set; } = new List<SourcePriority>();
        public virtual List<SourcePriority> IsExplicit { get; set; } = new List<SourcePriority>();
        public virtual List<SourcePriority> Covers { get; set; } = new List<SourcePriority>();
        public bool MergeCovers { get; set; } = true;
        public object this[string PropertyName]
        {
            get
            {
                Type myType = typeof(FieldPriorities);
                System.Reflection.PropertyInfo pi = myType.GetProperty(PropertyName);
                if (pi == null) return null;
                return pi.GetValue(this, null); //not indexed property!
            }
            set
            {
                Type myType = typeof(FieldPriorities);
                System.Reflection.PropertyInfo pi = myType.GetProperty(PropertyName);
                pi.SetValue(this, value, null); //not indexed property!
            }
        }

        public FieldPriorities()
        {

        }
    }
}