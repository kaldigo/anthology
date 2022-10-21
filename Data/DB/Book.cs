using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Anthology.Data.DB
{
    public class Book
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string ISBN { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public virtual List<BookAuthor> Authors { get; set; } = new List<BookAuthor>();
        public virtual List<BookNarrator> Narrators { get; set; } = new List<BookNarrator>();
        public virtual List<BookSeries> Series { get; set; } = new List<BookSeries>();
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public DateTime? PublishDate { get; set; }
        public virtual List<BookGenre> Genres { get; set; } = new List<BookGenre>();
        public virtual List<BookTag> Tags { get; set; } = new List<BookTag>();
        public string? Language { get; set; }
        public bool IsExplicit { get; set; } = false;
        public virtual List<BookCover> BookCovers { get; set; } = new List<BookCover>();
        public virtual List<AudiobookCover> AudiobookCovers { get; set; } = new List<AudiobookCover>();
        public virtual List<BookImage> Images { get; set; } = new List<BookImage>();
        public string GRID { get; set; }
        public string? ASIN { get; set; }
        public bool AudibleExists { get; set; } = true;
        public string? AGID { get; set; }
        public bool AudiobookGuildExists { get; set; } = true;
        public object this[string PropertyName]
        {
            get
            {
                Type myType = typeof(Book);
                System.Reflection.PropertyInfo pi = myType.GetProperty(PropertyName);
                return pi.GetValue(this, null); //not indexed property!
            }
            set
            {
                Type myType = typeof(Book);
                System.Reflection.PropertyInfo pi = myType.GetProperty(PropertyName);
                pi.SetValue(this, value, null); //not indexed property!
            }
        }
        public Book() { }
        public Book(string title, string author, string goodreadsId, DatabaseContext? context = null)
        {
            if (context == null) context = new DatabaseContext();

            this.ISBN = GenerateId(new DatabaseContext(), 12);
            this.Title = title;
            this.Authors = new List<BookAuthor>() { { new BookAuthor(author) } };
            this.GRID = goodreadsId;
        }
        public Book(string title, List<string> authors, string goodreadsId, DatabaseContext? context = null)
        {
            if (context == null) context = new DatabaseContext();

            this.ISBN = GenerateId(new DatabaseContext(), 12);
            this.Title = title;
            this.Authors = authors.Select(a => new BookAuthor(a)).ToList();
            this.GRID = goodreadsId;
        }

        public static string GenerateId(DatabaseContext? context = null)
        {
            if (context == null) context = new DatabaseContext();

            return GenerateId(new DatabaseContext(), 12);
        }
        private static string GenerateId(DatabaseContext context, int length)
        {
            string newID = null;

            while (newID == null)
            {
                Random random = new Random();
                string r = "";
                int i;
                for (i = 0; i < length; i++)
                {
                    r += random.Next(0, 9).ToString();
                }

                if (!context.Books.Any(b => b.ISBN == "ANTH" + r)) newID = "ANTH" + r;
            }

            return newID;
        }

        public Readarr.Book ReadarrMatch()
        {
            return Utils.Readarr.GetBook(GRID);
        }

        public List<Match> Matches(string source)
        {
            if (Title == null) return new List<Match>();

            var searchIds = new List<string>();
            string authorName = null;
            if (this.Authors.Count > 0) authorName = this.Authors.First().Name;
            switch (source)
            {
                case "Audible":
                    searchIds = Utils.Audible.Search(this.Title, authorName);
                    break;
                case "AudiobookGuild":
                    searchIds = Utils.AudiobookGuild.Search(this.Title);
                    break;
            }

            var searchBooks = new List<Match>();
            switch (source)
            {
                case "Audible":
                    foreach (var asin in searchIds)
                    {
                        var bookDetails = Utils.Audible.GetBook(asin);
                        if (bookDetails != null)
                        {
                            searchBooks.Add(
                                new Match()
                                {
                                    Identifier = bookDetails.asin,
                                    Title = bookDetails.title,
                                    Authors = String.Join(", ", bookDetails.authors.Select(a => a.name)),
                                    Series = bookDetails.seriesPrimary != null ? bookDetails.seriesPrimary.name + " #" + bookDetails.seriesPrimary.position : null,
                                    Image = bookDetails.image
                                });
                        }
                    }
                    break;
                case "AudiobookGuild":
                    foreach (var agid in searchIds)
                    {
                        var bookDetails = Utils.AudiobookGuild.GetBook(agid);
                        if (bookDetails != null)
                        {
                            searchBooks.Add(
                                new Match()
                                {
                                    Identifier = bookDetails.handle,
                                    Title = bookDetails.title,
                                    Authors = String.Join(", ", bookDetails.GetTags("Author")),
                                    Series = String.Join(", ", bookDetails.GetTags("Series")),
                                    Image = Regex.Unescape(bookDetails.images.First().src)
                                });
                        }
                    }
                    break;
            }

            return searchBooks;
        }
    }
}
