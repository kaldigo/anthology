using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Anthology.Data
{
    public class Settings : ICloneable
	{
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public virtual FieldPriorities FieldPriorities { get; set; } = new FieldPriorities();
        public virtual List<PluginSetting> PluginSettings { get; set; } = new List<PluginSetting>();
        public object this[string PropertyName]
        {
            get
            {
                Type myType = typeof(Book);
                System.Reflection.PropertyInfo pi = myType.GetProperty(PropertyName);
                return pi.GetValue(this, null); //not indexed property!
            }
            set
            {
                Type myType = typeof(Book);
                System.Reflection.PropertyInfo pi = myType.GetProperty(PropertyName);
                pi.SetValue(this, value, null); //not indexed property!
            }
		}
		public object Clone()
		{
			return this.MemberwiseClone();
		}
		public Settings() { }
    }
}
