namespace Anthology.Data.Readarr
{
    public class Edition
    {
        public int bookId { get; set; }
        public string foreignEditionId { get; set; }
        public string titleSlug { get; set; }
        public string isbn13 { get; set; }
        public string asin { get; set; }
        public string title { get; set; }
        public string overview { get; set; }
        public string format { get; set; }
        public bool isEbook { get; set; }
        public string disambiguation { get; set; }
        public string publisher { get; set; }
        public int pageCount { get; set; }
        public DateTime releaseDate { get; set; }
        public List<Image> images { get; set; }
        public List<Link> links { get; set; }
        public Ratings ratings { get; set; }
        public bool monitored { get; set; }
        public bool manualAdd { get; set; }
        public bool grabbed { get; set; }
        public int id { get; set; }
        public string language { get; set; }
    }
}