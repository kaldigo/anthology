using Anthology.Data.Audible;
using Anthology.Data.DB;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Anthology.Utils
{
    public static class BookFunnelUtils
    {
        public static void Login(BrowserSession b)
        {
            b.Get("https://my.bookfunnel.com/login");
            b.FormElements["email"] = Environment.GetEnvironmentVariable("BOOKFUNNEL_USERNAME");
            b.FormElements["password"] = Environment.GetEnvironmentVariable("BOOKFUNNEL_PASSWORD");
            b.Post("https://my.bookfunnel.com/login");
        }

        public static List<BookFunnel> GetBookList(BrowserSession b)
        {
            List<BookFunnel> books = new List<BookFunnel>();
            int pageLength = 48;
            int offset = 0;
            bool keepChecking = true;
            while (keepChecking)
            {
                List<BookFunnel> booksToAdd = GetBookListPage(b, offset);
                if (booksToAdd.Count != 0)
                {
                    books.AddRange(booksToAdd);
                    offset += pageLength;
                }
                else
                {
                    keepChecking = false;
                }
            }
            Console.WriteLine(books.Count + " Books Found");
            return books;
        }

        public static List<BookFunnel> GetBookListPage(BrowserSession b, int offset)
        {
            List<BookFunnel> books = new List<BookFunnel>();
            string pageHtml = b.Get("https://my.bookfunnel.com/books?sort=added&offset=" + offset);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            HtmlNodeCollection htmlNodes = htmlDoc.DocumentNode.SelectNodes("/div");

            if (htmlNodes != null)
            {
                foreach (HtmlNode node in htmlNodes)
                {
                    BookFunnel book = new BookFunnel();
                    book.ID = node.Attributes["data-id"].Value;
                    book.Title = node.SelectSingleNode(".//span[contains(@class, 'title')]").InnerText;
                    book.Author = node.SelectSingleNode(".//span[contains(@class, 'author')]").InnerText;
                    book.DateAdded = DateTime.Now.AddSeconds(-int.Parse(node.SelectSingleNode(".//span[contains(@class, 'added')]").InnerText));
                    books.Add(book);
                }
            }

            return books;
        }

        public static string GetBookDownloadPage(BrowserSession b, string downloadPageURL)
        {
            string pageHtml = b.Get(downloadPageURL + "_table");

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            HtmlNode htmlNode = htmlDoc.DocumentNode.SelectSingleNode("//a");

            return htmlNode.GetAttributeValue("href", null);
        }

    }
}
