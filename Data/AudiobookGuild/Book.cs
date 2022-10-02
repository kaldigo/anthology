using System.Text.RegularExpressions;

namespace Anthology.Data.AudiobookGuild
{
    public class Book
    {
        public object id { get; set; }
        public string title { get; set; }
        public string handle { get; set; }
        public string body_html { get; set; }
        public DateTime published_at { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string vendor { get; set; }
        public string product_type { get; set; }
        public string tags { get; set; }
        public List<Variant> variants { get; set; }
        public List<Image> images { get; set; }
        public List<Option> options { get; set; }

        //Possible Tags:
        //Author
        //Narrator
        //Series
        //Genre
        //Harem
        //Sexy Time
        //Trope
        public List<string> GetTags(string tag)
        {
            var tagList = new List<string>();

            foreach (string currentTag in this.tags.Split(", "))
            {
                var r = new Regex(@"(.+)_(.+)", RegexOptions.IgnoreCase);
                var m = r.Match(currentTag);
                if (m.Success && m.Groups[1].Value == tag) tagList.Add(Regex.Unescape(m.Groups[2].Value));
            }

            return tagList;
        }
    }
}