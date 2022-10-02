using Anthology.Data.Audible;
using Newtonsoft.Json;

namespace Anthology.Utils
{
    public class Audible
    {
        public static Book GetBook(string asin)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var url = "https://api.audnex.us/books/" + asin;
                    var response = client.GetStringAsync(url).Result;
                    if (response != null)
                    {
                        var jsonString = JsonConvert.DeserializeObject<Book>(response);
                        return jsonString;
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
        public static List<string> Search(string title)
        {
            using (HttpClient client = new HttpClient())
            {
                var url = "https://api.audible.com//1.0/catalog/products?num_results=25&products_sort_by=Relevance&title=" + title;
                var response = client.GetStringAsync(url).Result;
                if (response != null)
                {
                    var jsonString = JsonConvert.DeserializeObject<dynamic>(response);

                    var asinList = new List<string>();
                    foreach (var result in jsonString.products)
                    {
                        string resultAsin = result.asin;
                        asinList.Add(resultAsin);
                    }

                    return asinList;
                }
                else
                {
                    return null;
                }
            }
        }

    }
}
