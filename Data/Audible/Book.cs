namespace Anthology.Data.Audible
{
    public class Book
    {
        public string asin { get; set; }
        public List<Author> authors { get; set; } = new List<Author>();
        public string? description { get; set; }
        public string? formatType { get; set; }
        public string? image { get; set; }
        public string? language { get; set; }
        public List<Narrator> narrators { get; set; } = new List<Narrator>();
        public string? publisherName { get; set; }
        public string? rating { get; set; }
        public DateTime releaseDate { get; set; }
        public int runtimeLengthMin { get; set; }
        public Series? seriesPrimary { get; set; }
        public Series? seriesSecondary { get; set; }
        public string? subtitle { get; set; }
        public string? summary { get; set; }
        public string? title { get; set; }
        public List<Genre> genres { get; set; } = new List<Genre>();
    }
}