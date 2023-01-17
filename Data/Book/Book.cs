using Anthology.Plugins.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Anthology.Data
{
    public class Book
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string ISBN { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public bool SubtitleLock { get; set; } = false;
        public virtual List<Person> Authors { get; set; } = new List<Person>();
        public virtual List<Person> Narrators { get; set; } = new List<Person>();
        public virtual List<BookSeries> Series { get; set; } = new List<BookSeries>();
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public DateTime? PublishDate { get; set; }
        public virtual List<Classification> Classifications { get; set; } = new List<Classification>();
        public string? Language { get; set; }
        public bool? IsExplicit { get; set; }
        public virtual List<BookCover> BookCovers { get; set; } = new List<BookCover>();
        public virtual List<AudiobookCover> AudiobookCovers { get; set; } = new List<AudiobookCover>();
        public virtual List<BookImage> Images { get; set; } = new List<BookImage>();
        public virtual List<BookIdentifier> Identifiers { get; set; } = new List<BookIdentifier>();
        public string? BookMetadataJson { get; set; }
        [NotMapped]
        public Metadata BookMetadata
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.BookMetadataJson) ? new Metadata() : JsonConvert.DeserializeObject<Metadata>(this.BookMetadataJson);
            }
            set
            {

                this.BookMetadataJson = JsonConvert.SerializeObject(value, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }
        }
        public DateTime? DateFetchedMetadata { get; set; }
        [NotMapped]
        public bool ExistsInLibrary { get; set; } = false;
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
        public Book(string title, List<string> authors, string sourceKey, string sourceIdentifier, DatabaseContext? context = null)
        {
            this.ISBN = GenerateId(context);
            this.Title = title;
            this.Authors = authors.Select(a => new Person(a)).ToList();
            this.Identifiers = new List<BookIdentifier>() { new BookIdentifier(sourceKey, sourceIdentifier) };
        }
        public static string GenerateId(DatabaseContext? context = null)
        {
            if (context == null) context = new DatabaseContext();

            return GenerateId(context, 12);
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

        public void Update(Book book)
        {
            Title = book.Title;
            Subtitle = book.Subtitle;
            SubtitleLock = book.SubtitleLock;
            Authors = book.Authors;
            Narrators = book.Narrators;
            Series = book.Series;
            Description = book.Description;
            Publisher = book.Publisher;
            PublishDate = book.PublishDate;
            Classifications = book.Classifications;
            Language = book.Language;
            IsExplicit = book.IsExplicit;
            BookCovers = book.BookCovers;
            AudiobookCovers = book.AudiobookCovers;
            Images = book.Images;
            Identifiers = book.Identifiers.Select(i => i.Clone()).ToList();
            BookMetadata = book.BookMetadata;
            DateFetchedMetadata = book.DateFetchedMetadata;
            ExistsInLibrary = book.ExistsInLibrary;

        }

        public Book Clone()
        {
            var book = new Book()
            {
                ISBN = this.ISBN,
                Title = this.Title,
                Subtitle = this.Subtitle,
                SubtitleLock = this.SubtitleLock,
                Authors = this.Authors,
                Narrators = this.Narrators,
                Series = this.Series,
                Description = this.Description,
                Publisher = this.Publisher,
                PublishDate = this.PublishDate,
                Classifications = this.Classifications,
                Language = this.Language,
                IsExplicit = this.IsExplicit,
                BookCovers = this.BookCovers,
                AudiobookCovers = this.AudiobookCovers,
                Images = this.Images,
                Identifiers = this.Identifiers.Select(i => i.Clone()).ToList(),
                BookMetadata = this.BookMetadata,
                DateFetchedMetadata = this.DateFetchedMetadata,
                ExistsInLibrary = this.ExistsInLibrary,
            };
            return book;
        }
    }
}
