using Anthology.Data.AudiobookGuild;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Anthology.Utils
{
    public class AudiobookGuild
    {
        public static List<string> AGAuthors = new List<string>()
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
        public static Book GetBook(string agid)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
                var url = "https://audiobookguild.com/products/" + agid + ".json";
                var response = client.GetStringAsync(url).Result;
                if (response != null)
                {
                    var jsonString = JsonConvert.DeserializeObject<Root>(response);
                    return jsonString.product;
                }
                else
                {
                    return null;
                }
            }
        }
        public static List<string> Search(string title)
        {
            using (HttpClient client = new HttpClient())
            {
                var url = "https://audiobookguild.com/search?q=" + title + "&view=header";
                var response = client.GetStringAsync(url).Result;
                if (response != null)
                {
                    var jsonString = JsonConvert.DeserializeObject<SearchRoot>(response);

                    return jsonString.products.Select(r => r.handle).ToList();
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
