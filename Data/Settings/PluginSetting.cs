using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data
{
    public class PluginSetting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string PluginName { get; set; }
        public virtual List<SettingKV> Settings { get; set; } = new List<SettingKV>();
    }
}