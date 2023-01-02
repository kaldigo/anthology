using Anthology.Plugins;
using Anthology.Plugins.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Anthology.Plugins.MetadataSources
{
    internal class AudiobookGuild : IMetadataSource
    {
        public string Name => "AudiobookGuild";

        public string IdentifierKey => "AGID";

        public static List<string> AGAuthors => new List<string>()
        {
            "Dave Daren",
            "Duncan Wallace",
            "Eric Vall",
            "K. D. Robertson",
            "Logan Jacobs",
            "Michael-Scott Earle",
            "Nathan Thompson",
            "Robert M. Kerns"
        };

        public List<string> Settings => new List<string>();

        public Metadata GetMetadata(string identifier, Dictionary<string, string> settings)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
                    var url = "https://audiobookguild.com/products/" + identifier + ".json";
                    var response = client.GetStringAsync(url).Result;
                    if (response != null)
                    {
                        var jsonString = JsonConvert.DeserializeObject<AudiobookGuildRoot>(response);
                        return ConvertImportToMetadata(jsonString.product);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<Metadata> Search(Dictionary<string, string> settings, string title, string author = null)
        {
            using (HttpClient client = new HttpClient())
            {
                var url = "https://audiobookguild.com/search?q=" + title + "&view=header";
                var response = client.GetStringAsync(url).Result;
                if (response != null)
                {
                    var jsonString = JsonConvert.DeserializeObject<AudiobookGuildSearchRoot>(response);

                    return jsonString.products.Select(r => GetMetadata(r.handle, settings)).ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        private Metadata ConvertImportToMetadata(AudiobookGuildBook audiobookGuildBook)
        {
            var metadata = new Metadata();
            metadata.Title = audiobookGuildBook.title;
            metadata.Authors = audiobookGuildBook.GetTags("Author");
            metadata.Narrators = audiobookGuildBook.GetTags("Narrator");
            metadata.Series = audiobookGuildBook.GetTags("Series").Select(s => new Metadata.MetadataSeries(s, null)).ToList();
            metadata.Description = Regex.Replace(Regex.Unescape(audiobookGuildBook.body_html).Replace("<br>", "\n"), "<.*?>", string.Empty).Replace("\u00A0", " ").Replace("OVERVIEW:", "").Replace("Looking for the ebook?", "").Replace("Find it on Amazon", "").Trim().Trim('\r', '\n', ' ').Trim();
            metadata.Publisher = string.Join(", ", audiobookGuildBook.GetTags("Author"));
            metadata.PublishDate = audiobookGuildBook.published_at;
            metadata.Genres = audiobookGuildBook.GetTags("Genre");
            metadata.Tags = audiobookGuildBook.GetTags("Trope");
            metadata.Covers = audiobookGuildBook.images.Select(i => Regex.Unescape(i.src)).ToList();

            return metadata;
        }

        #region Import Model
        private class AudiobookGuildRoot
        {
            public AudiobookGuildBook? product { get; set; }
            public List<AudiobookGuildBook> products { get; set; } = new List<AudiobookGuildBook>();
        }
        private class AudiobookGuildBook
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
            public List<AudiobookGuildVariant> variants { get; set; }
            public List<AudiobookGuildImage> images { get; set; }
            public List<AudiobookGuildOption> options { get; set; }

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

                foreach (string currentTag in tags.Split(", "))
                {
                    var r = new Regex(@"(.+)_(.+)", RegexOptions.IgnoreCase);
                    var m = r.Match(currentTag);
                    if (m.Success && m.Groups[1].Value == tag) tagList.Add(Regex.Unescape(m.Groups[2].Value));
                }

                return tagList;
            }
        }
        private class AudiobookGuildVariant
        {
            public object id { get; set; }
            public string title { get; set; }
            public string option1 { get; set; }
            public object option2 { get; set; }
            public object option3 { get; set; }
            public string sku { get; set; }
            public bool requires_shipping { get; set; }
            public bool taxable { get; set; }
            public AudiobookGuildFeaturedImage featured_image { get; set; }
            public bool available { get; set; }
            public string price { get; set; }
            public int grams { get; set; }
            public object compare_at_price { get; set; }
            public int position { get; set; }
            public object product_id { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
        }
        private class AudiobookGuildImage
        {
            public object id { get; set; }
            public DateTime created_at { get; set; }
            public int position { get; set; }
            public DateTime updated_at { get; set; }
            public object product_id { get; set; }
            public List<object> variant_ids { get; set; }
            public string src { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }
        private class AudiobookGuildOption
        {
            public string name { get; set; }
            public int position { get; set; }
            public List<string> values { get; set; }
        }
        private class AudiobookGuildFeaturedImage
        {
            public object id { get; set; }
            public object product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public List<object> variant_ids { get; set; }
        }
        private class AudiobookGuildContent
        {
            public string title { get; set; }
            public string url { get; set; }
        }
        private class AudiobookGuildPrice
        {
            public int price { get; set; }
            public int price_min { get; set; }
            public int price_max { get; set; }
            public object compare_at_price { get; set; }
            public int compare_at_price_min { get; set; }
            public int compare_at_price_max { get; set; }
            public bool compare_at_price_varies { get; set; }
        }
        private class AudiobookGuildSearchBook
        {
            public string title { get; set; }
            public object id { get; set; }
            public string handle { get; set; }
            public bool on_sale { get; set; }
            public bool consistent_saved { get; set; }
            public AudiobookGuildPrice price { get; set; }
            public string image { get; set; }
            public string url { get; set; }
            public object swatch_count { get; set; }
        }
        private class AudiobookGuildSearchRoot
        {
            public List<AudiobookGuildContent> content { get; set; }
            public List<AudiobookGuildSearchBook> products { get; set; }
            public string terms { get; set; }
            public string sanitizedTerms { get; set; }
        }
        #endregion
    }
}
