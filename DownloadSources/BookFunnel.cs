using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.Text;
using Anthology.Plugins;
using Anthology.Plugins.Models;

namespace Anthology.Plugins.DownloadSources
{
    public class BookFunnel : IDownloadSource
    {
        public string Name => "Book Funnel";

        public string IdentifierKey => "BFID";

        public List<string> Settings => new List<string>() { "Username", "Password" };

        public bool DownloadBook(Download download, Dictionary<string, string> settings)
        {
            throw new NotImplementedException();
        }

        public List<Download> RefreshList(Dictionary<string, string> settings)
        {
            var b = new BrowserSession();
            Login(b, settings);

            return GetBookList(b);
        }
        public static void Login(BrowserSession b, Dictionary<string, string> settings)
        {
            b.Get("https://my.bookfunnel.com/login");
            b.FormElements["email"] = settings["Username"];
            b.FormElements["password"] = settings["Password"];
            b.Post("https://my.bookfunnel.com/login");
        }

        public List<Download> GetBookList(BrowserSession b)
        {
            List<Download> books = new List<Download>();
            int pageLength = 48;
            int offset = 0;
            bool keepChecking = true;
            while (keepChecking)
            {
                List<Download> booksToAdd = GetBookListPage(b, offset);
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
            return books;
        }

        public List<Download> GetBookListPage(BrowserSession b, int offset)
        {
            List<Download> books = new List<Download>();
            string pageHtml = b.Get("https://my.bookfunnel.com/books?sort=added&offset=" + offset);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            HtmlNodeCollection htmlNodes = htmlDoc.DocumentNode.SelectNodes("/div");

            if (htmlNodes != null)
            {
                foreach (HtmlNode node in htmlNodes)
                {
                    Download book = new Download();
                    book.Key = IdentifierKey;
                    book.Identifier = node.Attributes["data-id"].Value;
                    book.Title = node.SelectSingleNode(".//span[contains(@class, 'title')]").InnerText;
                    book.Author = new List<string>() { node.SelectSingleNode(".//span[contains(@class, 'author')]").InnerText };
                    book.ImageURL = node.SelectSingleNode(".//img[contains(@class, 'library-cover')]").GetAttributeValue("src", "");
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

        #region Import Model
        public class BrowserSession
        {
            private bool _isPost;
            private bool _isDownload;
            private HtmlDocument _htmlDoc;
            private string _download;

            /// <summary>
            /// System.Net.CookieCollection. Provides a collection container for instances of Cookie class 
            /// </summary>
            public CookieCollection Cookies { get; set; }

            /// <summary>
            /// Provide a key-value-pair collection of form elements 
            /// </summary>
            public FormElementCollection FormElements { get; set; }

            /// <summary>
            /// Makes a HTTP GET request to the given URL
            /// </summary>
            public string Get(string url)
            {
                _isPost = false;
                CreateWebRequestObject().Load(url);
                return _htmlDoc.DocumentNode.InnerHtml;
            }

            /// <summary>
            /// Makes a HTTP POST request to the given URL
            /// </summary>
            public string Post(string url)
            {
                _isPost = true;
                CreateWebRequestObject().Load(url, "POST");
                return _htmlDoc.DocumentNode.InnerHtml;
            }

            public string GetDownload(string url)
            {
                _isPost = false;
                _isDownload = true;
                CreateWebRequestObject().Load(url);
                return _download;
            }

            /// <summary>
            /// Creates the HtmlWeb object and initializes all event handlers. 
            /// </summary>
            private HtmlWeb CreateWebRequestObject()
            {
                HtmlWeb web = new HtmlWeb();
                web.UseCookies = true;
                web.PreRequest = new HtmlWeb.PreRequestHandler(OnPreRequest);
                web.PostResponse = new HtmlWeb.PostResponseHandler(OnAfterResponse);
                web.PreHandleDocument = new HtmlWeb.PreHandleDocumentHandler(OnPreHandleDocument);
                return web;
            }

            /// <summary>
            /// Event handler for HtmlWeb.PreRequestHandler. Occurs before an HTTP request is executed.
            /// </summary>
            protected bool OnPreRequest(HttpWebRequest request)
            {
                AddCookiesTo(request);               // Add cookies that were saved from previous requests
                if (_isPost) AddPostDataTo(request); // We only need to add post data on a POST request
                return true;
            }

            /// <summary>
            /// Event handler for HtmlWeb.PostResponseHandler. Occurs after a HTTP response is received
            /// </summary>
            protected void OnAfterResponse(HttpWebRequest request, HttpWebResponse response)
            {
                SaveCookiesFrom(request, response); // Save cookies for subsequent requests

                if (response != null && _isDownload)
                {
                    Stream remoteStream = response.GetResponseStream();
                    var sr = new StreamReader(remoteStream);
                    _download = sr.ReadToEnd();
                }
            }

            /// <summary>
            /// Event handler for HtmlWeb.PreHandleDocumentHandler. Occurs before a HTML document is handled
            /// </summary>
            protected void OnPreHandleDocument(HtmlDocument document)
            {
                SaveHtmlDocument(document);
            }

            /// <summary>
            /// Assembles the Post data and attaches to the request object
            /// </summary>
            private void AddPostDataTo(HttpWebRequest request)
            {
                string payload = FormElements.AssemblePostPayload();
                byte[] buff = Encoding.UTF8.GetBytes(payload.ToCharArray());
                request.ContentLength = buff.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                Stream reqStream = request.GetRequestStream();
                reqStream.Write(buff, 0, buff.Length);
            }

            /// <summary>
            /// Add cookies to the request object
            /// </summary>
            private void AddCookiesTo(HttpWebRequest request)
            {
                if (Cookies != null && Cookies.Count > 0)
                {
                    request.CookieContainer.Add(Cookies);
                }
            }

            /// <summary>
            /// Saves cookies from the response object to the local CookieCollection object
            /// </summary>
            private void SaveCookiesFrom(HttpWebRequest request, HttpWebResponse response)
            {
                //save the cookies ;)
                if (request.CookieContainer.Count > 0 || response.Cookies.Count > 0)
                {
                    if (Cookies == null)
                    {
                        Cookies = new CookieCollection();
                    }

                    Cookies.Add(request.CookieContainer.GetCookies(request.RequestUri));
                    Cookies.Add(response.Cookies);
                }
            }

            /// <summary>
            /// Saves the form elements collection by parsing the HTML document
            /// </summary>
            private void SaveHtmlDocument(HtmlDocument document)
            {
                _htmlDoc = document;
                FormElements = new FormElementCollection(_htmlDoc);
            }
        }

        /// <summary>
        /// Represents a combined list and collection of Form Elements.
        /// </summary>
        public class FormElementCollection : Dictionary<string, string>
        {
            /// <summary>
            /// Constructor. Parses the HtmlDocument to get all form input elements. 
            /// </summary>
            public FormElementCollection(HtmlDocument htmlDoc)
            {
                var inputs = htmlDoc.DocumentNode.Descendants("input");
                foreach (var element in inputs)
                {
                    string name = element.GetAttributeValue("name", "undefined");
                    string value = element.GetAttributeValue("value", "");

                    if (!ContainsKey(name))
                    {
                        if (!name.Equals("undefined"))
                        {
                            Add(name, value);
                        }
                    }
                }
            }

            /// <summary>
            /// Assembles all form elements and values to POST. Also html encodes the values.  
            /// </summary>
            public string AssemblePostPayload()
            {
                StringBuilder sb = new StringBuilder();
                foreach (var element in this)
                {
                    string value = System.Web.HttpUtility.UrlEncode(element.Value);
                    sb.Append("&" + element.Key + "=" + value);
                }
                return sb.ToString().Substring(1);
            }
        }
        #endregion
    }
}
