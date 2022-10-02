using Anthology.Data.Readarr;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Series = Anthology.Data.Metadata.Series;

namespace Anthology.Utils
{
    public static class Readarr
    {
        public static List<Book> GetBooks()
        {
            using (HttpClient client = new HttpClient())
            {
                var url = Environment.GetEnvironmentVariable("READARR_URL") + "/api/v1/book";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("READARR_TOKEN"));
                var response = client.GetStringAsync(url).Result;
                if (response != null)
                {
                    var jsonString = JsonConvert.DeserializeObject<List<Book>>(response);
                    return jsonString.Where(b => b.monitored).ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        public static Book GetBook(string grid)
        {
            using (HttpClient client = new HttpClient())
            {
                var url = Environment.GetEnvironmentVariable("READARR_URL") + "/api/v1/book/lookup?term=work:" + grid;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("READARR_TOKEN"));
                var response = client.GetStringAsync(url).Result;
                if (response != null)
                {
                    var jsonString = JsonConvert.DeserializeObject<List<Book>>(response);
                    return jsonString[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public static List<Series> ExtractSeries(List<string> seriesList)
        {
            var seriesDict = new List<Series>();
            foreach (var item in seriesList)
            {
                var series = Regex.Match(item.Trim(), @"(.+) #(.+)").Groups[1].Value.Trim();
                var indexString = Regex.Match(item.Trim(), @"(.+) #(.+)").Groups[2].Value.Trim();

                var bookSeries = new Series();
                if (string.IsNullOrEmpty(series)) series = item.Trim();
                bookSeries.Name = series;
                if (!String.IsNullOrEmpty(indexString)) bookSeries.Sequence = float.Parse(indexString);

                seriesDict.Add(bookSeries);
            }
            return seriesDict;
        }
    }
}
