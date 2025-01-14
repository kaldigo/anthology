using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Anthology.Plugins;
using Anthology.Plugins.Models;
using HtmlAgilityPack;

namespace Anthology.Plugins.DownloadSources
{
    public class BookFunnel : IDownloadSource
    {
        public string Name => "Book Funnel";
        public string IdentifierKey => "BFID";
        public List<string> Settings => new List<string> { "Username", "Password" };

        private static Task _downloadTask;
        private static readonly List<Download> _downloadQueue = new List<Download>();

        private static Task _extractTask;
        private static readonly List<Download> _extractQueue = new List<Download>();

        #region IDownloadSource Implementation

        public bool DownloadBook(Download download, string mediaPath, Dictionary<string, string> settings)
        {
            return DownloadBookTask(mediaPath, settings, download);
        }

        public List<Download> GetDownloadList(Dictionary<string, string> settings)
        {
            var browser = new BrowserSession();
            Login(browser, settings);
            return GetBookList(browser);
        }

        #endregion

        #region Internal Download/Extract Logic

        private bool DownloadBookTask(string mediaPath, Dictionary<string, string> settings, Download download = null)
        {
            // If a new download was passed, add it if not already in the queue
            if (download != null && !_downloadQueue.Select(d => d.Identifier).Contains(download.Identifier))
            {
                _downloadQueue.Add(download);
            }

            // If a download task is still running or waiting, bail out
            if (_downloadTask != null &&
               (_downloadTask.Status == TaskStatus.Running ||
                _downloadTask.Status == TaskStatus.WaitingToRun ||
                _downloadTask.Status == TaskStatus.WaitingForActivation))
            {
                return true;
            }

            // If no downloads are pending, exit
            if (_downloadQueue.Count == 0) return true;

            // Take the first item in the queue
            var currentDownload = _downloadQueue.First();

            // Start the download process
            _downloadTask = Task.Factory.StartNew(() =>
            {
                var browser = new BrowserSession();
                Login(browser, settings);

                var downloadUrl = GetBookDownloadPage(browser, currentDownload.Identifier);

                var zipPath = Path.Combine(
                    mediaPath,
                    $"{CleanFileName(string.Join(", ", currentDownload.Author).Trim())} - {CleanFileName(currentDownload.Title.Trim())}.zip"
                );

                using (var client = new WebClient())
                {
                    client.DownloadFile(downloadUrl, zipPath);
                }
            });

            // Block until download finishes
            _downloadTask.Wait();

            // Once downloaded, extract the book
            ExtractBookTask(mediaPath, currentDownload);

            // Remove from queue
            _downloadQueue.Remove(currentDownload);

            // If there are more downloads, recursively process them
            DownloadBookTask(mediaPath, settings);

            return true;
        }

        private async Task<bool> ExtractBookTask(string mediaPath, Download download = null)
        {
            // If a new download was passed, add it if not already in the queue
            if (download != null && !_extractQueue.Select(d => d.Identifier).Contains(download.Identifier))
            {
                _extractQueue.Add(download);
            }

            // If an extract task is still running or waiting, bail out
            if (_extractTask != null &&
               (_extractTask.Status == TaskStatus.Running ||
                _extractTask.Status == TaskStatus.WaitingToRun ||
                _extractTask.Status == TaskStatus.WaitingForActivation))
            {
                return true;
            }

            // If no extracts are pending, exit
            if (_extractQueue.Count == 0) return true;

            // Take the first item in the queue
            var currentDownload = _extractQueue.First();

            // Start the extraction process
            _extractTask = Task.Factory.StartNew(() =>
            {
                var zipPath = Path.Combine(
                    mediaPath,
                    $"{CleanFileName(string.Join(", ", currentDownload.Author).Trim())} - {CleanFileName(currentDownload.Title.Trim())}.zip"
                );

                var itemPath = Path.Combine(
                    mediaPath,
                    CleanFileName(string.Join(", ", currentDownload.Author).Trim()),
                    CleanFileName(currentDownload.Title.Trim())
                );

                Directory.CreateDirectory(itemPath);

                // Extract ZIP to designated folder
                ZipFile.ExtractToDirectory(zipPath, itemPath);

                // Remove the ZIP file after extraction
                File.Delete(zipPath);

                // Clean up MP3 file names
                foreach (var filePath in Directory.GetFiles(itemPath))
                {
                    if (Path.GetExtension(filePath).Equals(".mp3", StringComparison.OrdinalIgnoreCase))
                    {
                        var fileName = Path.GetFileName(filePath);
                        var newFileName = Regex.Replace(fileName, @"\d{3} .+? - .+? - ", "");
                        var newPath = Path.Combine(itemPath, newFileName);
                        File.Move(filePath, newPath);
                    }
                }
            });

            // Block until extraction finishes
            _extractTask.Wait();

            // Remove from queue
            _extractQueue.Remove(currentDownload);

            // If more extracts are pending, recurse
            await ExtractBookTask(mediaPath);

            return true;
        }

        #endregion

        #region BookFunnel-specific Logic

        public static void Login(BrowserSession browser, Dictionary<string, string> settings)
        {
            const string loginUrl = "https://my.bookfunnel.com/login";

            var request = (HttpWebRequest)WebRequest.Create(loginUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.AllowAutoRedirect = false;
            request.CookieContainer = new CookieContainer();

            // Set headers
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                                "Chrome/91.0.4472.124 Safari/537.36";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Headers.Add("Accept-Language", "en-US,en;q=0.5");

            // Prepare form data
            var formData = $"email={Uri.EscapeDataString(settings["Username"])}" +
                           $"&password={Uri.EscapeDataString(settings["Password"])}";
            var formDataBytes = Encoding.UTF8.GetBytes(formData);

            // Write form data
            using (var stream = request.GetRequestStream())
            {
                stream.Write(formDataBytes, 0, formDataBytes.Length);
            }

            // Send request
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                // Handle manual redirects
                if ((response.StatusCode == HttpStatusCode.Found ||
                     response.StatusCode == HttpStatusCode.Redirect) &&
                     response.Headers["Set-Cookie"] != null)
                {
                    foreach (var cookieHeader in response.Headers.GetValues("Set-Cookie"))
                    {
                        var cookie = ParseCookie(cookieHeader, request.RequestUri.Host);
                        if (cookie != null)
                        {
                            browser.Cookies ??= new CookieCollection();
                            browser.Cookies.Add(cookie);
                        }
                    }
                }
            }
        }

        private static Cookie ParseCookie(string rawCookie, string domain)
        {
            try
            {
                if (string.IsNullOrEmpty(rawCookie)) return null;

                var parts = rawCookie.Split(';');
                var nameValue = parts[0].Split('=');

                if (nameValue.Length != 2) return null;

                var name = nameValue[0].Trim();
                var value = nameValue[1].Trim();
                if (string.IsNullOrEmpty(name)) return null;

                var cookie = new Cookie(name, value) { Domain = domain };

                // Parse additional attributes
                foreach (var part in parts.Skip(1))
                {
                    var attribute = part.Trim().ToLower();
                    if (attribute == "secure") cookie.Secure = true;
                    else if (attribute == "httponly") { /* default in .NET cookie container */ }
                    else if (attribute.StartsWith("expires="))
                    {
                        if (DateTime.TryParse(attribute.Substring(8), out var expiry))
                        {
                            cookie.Expires = expiry;
                        }
                    }
                    else if (attribute.StartsWith("path="))
                    {
                        cookie.Path = attribute.Substring(5);
                    }
                }

                return cookie;
            }
            catch
            {
                return null;
            }
        }

        public List<Download> GetBookList(BrowserSession browser)
        {
            var books = new List<Download>();
            const int pageLength = 48;
            var offset = 0;
            var keepChecking = true;

            while (keepChecking)
            {
                var booksToAdd = GetBookListPage(browser, offset);
                if (booksToAdd.Count > 0)
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

        public List<Download> GetBookListPage(BrowserSession browser, int offset)
        {
            var books = new List<Download>();
            var pageHtml = browser.Get($"https://my.bookfunnel.com/books?sort=added&offset={offset}");

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            var htmlNodes = htmlDoc.DocumentNode.SelectNodes("/div");
            if (htmlNodes == null) return books;

            foreach (var node in htmlNodes)
            {
                var book = new Download
                {
                    Key = IdentifierKey,
                    Identifier = node.Attributes["data-id"]?.Value,
                    Title = node.SelectSingleNode(".//span[contains(@class, 'title')]")?.InnerText ?? "",
                    Author = new List<string>
                    {
                        node.SelectSingleNode(".//span[contains(@class, 'author')]")?.InnerText ?? ""
                    },
                    ImageURL = node.SelectSingleNode(".//img[contains(@class, 'library-cover')]")
                                      ?.GetAttributeValue("src", ""),
                    DateAdded = DateTime.Now.AddSeconds(
                        -int.Parse(node.SelectSingleNode(".//span[contains(@class, 'added')]")?.InnerText ?? "0"))
                };

                books.Add(book);
            }
            return books;
        }

        public static string GetBookDownloadPage(BrowserSession browser, string bookId)
        {
            var pageHtml = browser.Get($"https://my.bookfunnel.com/{bookId}/download_table");
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            var linkNode = htmlDoc.DocumentNode.SelectSingleNode("//a");
            return linkNode?.GetAttributeValue("href", null);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Removes invalid characters from filenames.
        /// </summary>
        private static string CleanFileName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", name.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion

        #region Nested Classes

        public class BrowserSession
        {
            private bool _isPost;
            private HtmlDocument _htmlDoc;

            public CookieCollection Cookies { get; set; }
            public FormElementCollection FormElements { get; set; }

            public string Get(string url)
            {
                _isPost = false;
                CreateWebRequestObject().Load(url);
                return _htmlDoc.DocumentNode.InnerHtml;
            }

            public string Post(string url)
            {
                _isPost = true;
                CreateWebRequestObject().Load(url, "POST");
                return _htmlDoc.DocumentNode.InnerHtml;
            }

            private HtmlWeb CreateWebRequestObject()
            {
                var web = new HtmlWeb
                {
                    UseCookies = true,
                    PreRequest = OnPreRequest,
                    PostResponse = OnAfterResponse,
                    PreHandleDocument = OnPreHandleDocument
                };
                return web;
            }

            protected bool OnPreRequest(HttpWebRequest request)
            {
                AddCookiesTo(request);
                if (_isPost) AddPostDataTo(request);
                return true;
            }

            protected void OnAfterResponse(HttpWebRequest request, HttpWebResponse response)
            {
                SaveCookiesFrom(request, response);
            }

            protected void OnPreHandleDocument(HtmlDocument document)
            {
                _htmlDoc = document;
                FormElements = new FormElementCollection(_htmlDoc);
            }

            private void AddPostDataTo(HttpWebRequest request)
            {
                var payload = FormElements.AssemblePostPayload();
                var buff = Encoding.UTF8.GetBytes(payload);
                request.ContentLength = buff.Length;
                request.ContentType = "application/x-www-form-urlencoded";

                using var reqStream = request.GetRequestStream();
                reqStream.Write(buff, 0, buff.Length);
            }

            private void AddCookiesTo(HttpWebRequest request)
            {
                if (Cookies != null && Cookies.Count > 0)
                {
                    request.CookieContainer.Add(Cookies);
                }
            }

            private void SaveCookiesFrom(HttpWebRequest request, HttpWebResponse response)
            {
                if (request.CookieContainer.Count > 0 || response.Cookies.Count > 0)
                {
                    Cookies ??= new CookieCollection();
                    Cookies.Add(request.CookieContainer.GetCookies(request.RequestUri));
                    Cookies.Add(response.Cookies);
                }
            }
        }

        public class FormElementCollection : Dictionary<string, string>
        {
            public FormElementCollection(HtmlDocument htmlDoc)
            {
                var inputs = htmlDoc.DocumentNode.Descendants("input");
                foreach (var element in inputs)
                {
                    var name = element.GetAttributeValue("name", "undefined");
                    var value = element.GetAttributeValue("value", "");

                    if (!ContainsKey(name) &&
                        !name.Equals("undefined", StringComparison.OrdinalIgnoreCase))
                    {
                        Add(name, value);
                    }
                }
            }

            public string AssemblePostPayload()
            {
                var sb = new StringBuilder();
                foreach (var element in this)
                {
                    var value = System.Web.HttpUtility.UrlEncode(element.Value);
                    sb.Append("&" + element.Key + "=" + value);
                }

                // Remove the leading "&"
                return sb.Length > 0 ? sb.ToString()[1..] : string.Empty;
            }
        }

        #endregion
    }
}
