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
            var books = context.Books.Where(b => b.Title.Contains(title) || title.Contains(b.Title)).Select(b => GetBookMetadata(b.ISBN, isAudiobook, null).Result).ToList();
            return Task.FromResult(books);
        }
        public static async Task<Book?> GetBookMetadata(string isbn, bool isAudiobook, string? asin = null)
        {
            var context = new DatabaseContext();

            var dbBook = context.Books.FirstOrDefault(b => b.ISBN == isbn);
            if(dbBook == null) return null;

            if (asin != null && asin != dbBook.ASIN)
            {
                dbBook.ASIN = asin;
                context.SaveChanges();
            }

            Dictionary<string, Task<Book>> dataToFetch = new Dictionary<string, Task<Book>>();

            if (dbBook.GRID != null)
            {
                dataToFetch.Add("Goodreads", GetGoodreadsMetadata(dbBook.GRID));
            }
            if (dbBook.ASIN != null)
            {
                dataToFetch.Add("Audible", GetAudibleMetadata(dbBook.ASIN));
            }
            if (dbBook.AGID != null)
            {
                dataToFetch.Add("AudiobookGuild", GetAudiobookGuildMetadata(dbBook.AGID));
            }

            await Task.WhenAll(dataToFetch.Select(t => t.Value));

            Dictionary<string, Book> dataToMerge = dataToFetch.ToDictionary(d => d.Key, d => d.Value.Result);
            dataToMerge.Add("Override", new Book(dbBook));

            return MergeMetadata(dataToMerge, isAudiobook);
        }

        public static Task<Book> GetGoodreadsMetadata(string grid)
        {
            return Task.FromResult(new Book(Readarr.GetBook(grid)));
        }

        public static Task<Book> GetAudibleMetadata(string asin)
        {
            return Task.FromResult(new Book(Audible.GetBook(asin)));
        }

        public static Task<Book> GetAudiobookGuildMetadata(string agid)
        {
            return Task.FromResult(new Book(AudiobookGuild.GetBook(agid)));
        }

        public static Book MergeMetadata(Dictionary<string,Book> sources, bool isAudiobook)
        {
            var metadata = new Book();

            metadata.ISBN = MetadataUtils.SelectString("ISBN", sources, false);

            metadata.ASIN = MetadataUtils.SelectString("ASIN", sources, isAudiobook);

            metadata.Title = MetadataUtils.SelectString("Title", sources, isAudiobook);

            metadata.Subtitle = MetadataUtils.SelectString("Subtitle", sources, isAudiobook);

            metadata.Authors = MetadataUtils.SelectListString("Authors", sources, isAudiobook);

            if (isAudiobook) metadata.Narrators = MetadataUtils.SelectListString("Narrators", sources, isAudiobook);

            metadata.Series = MetadataUtils.SelectSeries("Series", sources, false);

            metadata.Description = MetadataUtils.SelectString("Description", sources, isAudiobook);

            metadata.Publisher = MetadataUtils.SelectString("Publisher", sources, isAudiobook);

            metadata.PublishDate = MetadataUtils.SelectDateTime("PublishDate", sources, isAudiobook);

            var rawClassificationList = new List<string>();
            rawClassificationList.AddRange(MetadataUtils.CombineListString("Genres", sources));
            rawClassificationList.AddRange(MetadataUtils.CombineListString("Tags", sources));

            var classificationList = Classification.GetClassification(rawClassificationList);

            if (metadata.Genres == null) metadata.Genres = new List<string>();
            metadata.Genres = classificationList.Where(c => c.Type == Classification.ClassificationType.Genre).Select(c => c.Name).ToList();

            if (metadata.Tags == null) metadata.Tags = new List<string>();
            metadata.Tags = classificationList.Where(c => c.Type == Classification.ClassificationType.Tag).Select(c => c.Name).ToList();

            metadata.Language = MetadataUtils.SelectString("Language", sources, isAudiobook);

            metadata.IsExplicit = MetadataUtils.SelectBool("IsExplicit", sources, isAudiobook);
            if (!metadata.IsExplicit && metadata.Genres != null) metadata.IsExplicit = classificationList.Any(c => c.Name.ToLower() == "adult" || c.Name.ToLower() == "erotica" || c.Name.ToLower() == "explicit");

            if (metadata.Covers == null) metadata.Covers = new List<string>();
            metadata.Covers = MetadataUtils.CombineListString("Covers", sources);

            return metadata;

        }
    }
}
