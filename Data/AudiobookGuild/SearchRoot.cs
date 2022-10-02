namespace Anthology.Data.AudiobookGuild
{
    public class SearchRoot
    {
        public List<Content> content { get; set; }
        public List<SearchBook> products { get; set; }
        public string terms { get; set; }
        public string sanitizedTerms { get; set; }
    }
}