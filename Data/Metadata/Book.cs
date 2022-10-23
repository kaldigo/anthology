using MetadataUtils =  Anthology.Utils.Metadata;
using ReadarrBook = Anthology.Data.Readarr.Book;
using AudibleBook = Anthology.Data.Audible.Book;
using AudiobookGuildBook = Anthology.Data.AudiobookGuild.Book;
using DBBook = Anthology.Data.DB.Book;

namespace Anthology.Data.Metadata
{
    public class Book
    {
        public string ISBN { get; set; }
        public string? ASIN { get; set; }
        public string Title { get; set; }
        public string? Subtitle { get; set; }
        public List<string> Authors { get; set; } = new List<string>();
        public List<string> Narrators { get; set; } = new List<string>();
        public List<Series> Series { get; set; } = new List<Series>();
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public DateTime? PublishDate { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public string? Language { get; set; }
        public bool IsExplicit { get; set; } = false;
        public List<string> Covers { get; set; } = new List<string>();
        public object this[string PropertyName]
        {
            get
            {
                Type myType = typeof(Book);
                System.Reflection.PropertyInfo pi = myType.GetProperty(PropertyName);
                if(pi == null) return null;
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
        public Book(DBBook dbBook)
        {
            MetadataUtils.ConvertDBBook(this, dbBook);
        }
        public Book(ReadarrBook readarrBook)
        {
            MetadataUtils.ConvertReadarrBook(this, readarrBook);
        }
        public Book(AudibleBook audibleBook)
        {
            MetadataUtils.ConvertAudibleBook(this, audibleBook);
        }
        public Book(AudiobookGuildBook audiobookGuildBook)
        {
            MetadataUtils.ConvertAudiobookGuildBook(this, audiobookGuildBook);
        }
    }
}