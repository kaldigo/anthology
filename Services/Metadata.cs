using Anthology.Data.DB;
using Anthology.Utils;
using Book = Anthology.Data.Metadata.Book;
using MetadataUtils = Anthology.Utils.Metadata;

namespace Anthology.Services
{
    public static class MetadataService
    {
        public static Task<List<Book>> Search(string title, string? author, string? isbn, string? asin, bool isAudiobook)
        {
            if (title.Length > 4 && title.Substring(0, 4) == "ANTH") isbn = title;

            if (isbn != null && isbn.Substring(0, 4) == "ANTH")
            {
                var searchISBN = MetadataService.GetBookMetadata(isbn, isAudiobook, asin).Result;
                if (searchISBN != null)
                {
                    var searchISBNList = new List<Book>();
                    searchISBNList.Add(searchISBN);
                    return Task.FromResult(searchISBNList);
                }
            }

            var context = new DatabaseContext();
            var books = context.Books.Where(b => b.Title.Contains(title) || title.Contains(b.Title)).Select(b => GetBookMetadata(b.ISBN, isAudiobook, null, false).Result).ToList();
            return Task.FromResult(books);
        }
        public static async Task<Book?> GetBookMetadata(string isbn, bool isAudiobook, string? asin = null, bool forceRefresh = false)
        {
            var context = new DatabaseContext();

            var dbBook = context.Books.FirstOrDefault(b => b.ISBN == isbn);
            if(dbBook == null) return null;

            if (asin != null && asin != dbBook.ASIN)
            {
                dbBook.ASIN = asin;
                context.SaveChanges();
            }

            if(dbBook.DateFetchedMetadata == null  || dbBook.DateFetchedMetadata  > DateTime.Now.AddDays(30) || forceRefresh) 
                RefreshBookMetadata(dbBook, context);

            Dictionary<string, Book> dataToMerge = new Dictionary<string, Book>();
            dataToMerge.Add("Override", new Book(dbBook));
            if(isAudiobook) dataToMerge.Add("Metadata", dbBook.AudioBookMetadata);
            else dataToMerge.Add("Metadata", dbBook.BookMetadata);

            return MergeMetadata(dataToMerge, isAudiobook);
        }
        public static void RefreshBookMetadata(Data.DB.Book dbBook,DatabaseContext context = null)
        {
            if(context == null) context = new DatabaseContext();
            dbBook.BookMetadata = FetchBookMetadata(dbBook, false, true).Result;
            dbBook.AudioBookMetadata = FetchBookMetadata(dbBook, true, true).Result;
            dbBook.DateFetchedMetadata = DateTime.Now;
            context.SaveChanges();
        }
        public static async Task<Book?> FetchBookMetadata(Data.DB.Book dbBook, bool isAudiobook, bool ignoreOveride)
        {
            Dictionary<string, Task<Book>> dataToFetch = new Dictionary<string, Task<Book>>();

            if (dbBook.GRID != null)
            {
                var bookMetadata = GetGoodreadsMetadata(dbBook.GRID);
                if (bookMetadata != null) dataToFetch.Add("Goodreads", bookMetadata);
            }
            if (dbBook.ASIN != null)
            {
                var bookMetadata = GetAudibleMetadata(dbBook.ASIN);
                if (bookMetadata != null) dataToFetch.Add("Audible", bookMetadata);
            }
            if (dbBook.AGID != null)
            {
                var bookMetadata = GetAudiobookGuildMetadata(dbBook.AGID);
                if (bookMetadata != null) dataToFetch.Add("AudiobookGuild", bookMetadata);
            }

            await Task.WhenAll(dataToFetch.Select(t => t.Value));

            Dictionary<string, Book> dataToMerge = dataToFetch.ToDictionary(d => d.Key, d => d.Value.Result);
            dataToMerge.Add("Override", new Book(dbBook));

            return MergeMetadata(dataToMerge, isAudiobook, ignoreOveride);
        }

        public static Task<Book> GetGoodreadsMetadata(string grid)
        {
            var book = Readarr.GetBook(grid);
            if (book == null) return null;
            return Task.FromResult(new Book(book));
        }

        public static Task<Book> GetAudibleMetadata(string asin)
        {
            var book = Audible.GetBook(asin);
            if(book == null) return null;
            return Task.FromResult(new Book(book));
        }

        public static Task<Book> GetAudiobookGuildMetadata(string agid)
        {
            var book = AudiobookGuild.GetBook(agid);
            if (book == null) return null;
            return Task.FromResult(new Book(book));
        }

        public static Book MergeMetadata(Dictionary<string,Book> sources, bool isAudiobook, bool ignoreOveride = false)
        {
            var metadata = new Book();

            metadata.ISBN = MetadataUtils.SelectString("ISBN", sources, false, ignoreOveride);

            metadata.ASIN = MetadataUtils.SelectString("ASIN", sources, isAudiobook, ignoreOveride);

            metadata.Title = MetadataUtils.SelectString("Title", sources, isAudiobook, ignoreOveride);

            metadata.Subtitle = MetadataUtils.SelectString("Subtitle", sources, isAudiobook, ignoreOveride);

            metadata.Authors = MetadataUtils.SelectListString("Authors", sources, isAudiobook, ignoreOveride);

            if (isAudiobook) metadata.Narrators = MetadataUtils.SelectListString("Narrators", sources, isAudiobook, ignoreOveride);

            metadata.Series = MetadataUtils.SelectSeries("Series", sources, false, ignoreOveride);

            metadata.Description = MetadataUtils.SelectString("Description", sources, isAudiobook, ignoreOveride);

            metadata.Publisher = MetadataUtils.SelectString("Publisher", sources, isAudiobook, ignoreOveride);

            metadata.PublishDate = MetadataUtils.SelectDateTime("PublishDate", sources, isAudiobook, ignoreOveride);

            var rawClassificationList = new List<string>();
            rawClassificationList.AddRange(MetadataUtils.CombineListString("Genres", sources, ignoreOveride));
            rawClassificationList.AddRange(MetadataUtils.CombineListString("Tags", sources, ignoreOveride));

            var classificationList = Classification.GetClassification(rawClassificationList);

            if (metadata.Genres == null) metadata.Genres = new List<string>();
            metadata.Genres = classificationList.Where(c => c.Type == Classification.ClassificationType.Genre).Select(c => c.Name).ToList();

            if (metadata.Tags == null) metadata.Tags = new List<string>();
            metadata.Tags = classificationList.Where(c => c.Type == Classification.ClassificationType.Tag).Select(c => c.Name).ToList();

            metadata.Language = MetadataUtils.SelectString("Language", sources, isAudiobook, ignoreOveride);

            metadata.IsExplicit = MetadataUtils.SelectBool("IsExplicit", sources, isAudiobook, ignoreOveride);
            if (!metadata.IsExplicit && metadata.Genres != null) metadata.IsExplicit = classificationList.Any(c => c.Name.ToLower() == "adult" || c.Name.ToLower() == "erotica" || c.Name.ToLower() == "explicit");

            if (metadata.Covers == null) metadata.Covers = new List<string>();
            metadata.Covers = MetadataUtils.CombineListString("Covers", sources, ignoreOveride);

            return metadata;

        }
    }
}
