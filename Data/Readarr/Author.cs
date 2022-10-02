namespace Anthology.Data.Readarr
{
    public class Author
    {
        public int authorMetadataId { get; set; }
        public string status { get; set; }
        public bool ended { get; set; }
        public string authorName { get; set; }
        public string authorNameLastFirst { get; set; }
        public string foreignAuthorId { get; set; }
        public string titleSlug { get; set; }
        public string overview { get; set; }
        public List<Link> links { get; set; }
        public List<object> images { get; set; }
        public string path { get; set; }
        public int qualityProfileId { get; set; }
        public int metadataProfileId { get; set; }
        public bool monitored { get; set; }
        public string monitorNewItems { get; set; }
        public List<object> genres { get; set; }
        public string cleanName { get; set; }
        public string sortName { get; set; }
        public string sortNameLastFirst { get; set; }
        public List<object> tags { get; set; }
        public DateTime added { get; set; }
        public Ratings ratings { get; set; }
        public Statistics statistics { get; set; }
        public int id { get; set; }
    }
}