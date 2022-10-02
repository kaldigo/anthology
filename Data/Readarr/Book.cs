namespace Anthology.Data.Readarr
{
    public class Book
    {
        public string title { get; set; }
        public string authorTitle { get; set; }
        public string seriesTitle { get; set; }
        public string disambiguation { get; set; }
        public string overview { get; set; }
        public int authorId { get; set; }
        public string foreignBookId { get; set; }
        public string titleSlug { get; set; }
        public bool monitored { get; set; }
        public bool anyEditionOk { get; set; }
        public Ratings ratings { get; set; }
        public DateTime releaseDate { get; set; }
        public int pageCount { get; set; }
        public List<string> genres { get; set; }
        public Author author { get; set; }
        public List<Image> images { get; set; }
        public List<Link> links { get; set; }
        public Statistics statistics { get; set; }
        public DateTime added { get; set; }
        public List<Edition> editions { get; set; }
        public bool grabbed { get; set; }
        public int id { get; set; }
    }
}