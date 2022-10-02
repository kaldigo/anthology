namespace Anthology.Data.AudiobookGuild
{
    public class Root
    {
        public Book? product { get; set; }
        public List<Book> products { get; set; } = new List<Book>();
    }
}