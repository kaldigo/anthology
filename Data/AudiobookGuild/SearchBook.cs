namespace Anthology.Data.AudiobookGuild
{
    public class SearchBook
    {
        public string title { get; set; }
        public object id { get; set; }
        public string handle { get; set; }
        public bool on_sale { get; set; }
        public bool consistent_saved { get; set; }
        public Price price { get; set; }
        public string image { get; set; }
        public string url { get; set; }
        public object swatch_count { get; set; }
    }
}