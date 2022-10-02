using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data.DB
{
    public abstract class AnthologyImage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string FileName { get; set; }
        public virtual Book Book { get; set; }
        public AnthologyImage() { }
        public AnthologyImage(string fileName)
        {
            this.FileName = fileName;
        }

        public abstract string GetFolderName();

        public string GetUrl()
        {
            return GetUrl(Book.ISBN);
        }
        public string GetUrl(string isbn)
        {
            return Environment.GetEnvironmentVariable("ANTHOLOGY_URL") + "/api/Image/" + isbn + "/" + GetFolderName().Replace(" ", "") + "/" + FileName;
        }

        public string GetPath()
        {
            return GetPath(this.Book.ISBN);
        }

        public string GetPath(string isbn)
        {
            return Path.Combine(Utils.FileUtils.GetConfigPath(), "Images", isbn, GetFolderName(), FileName);
        }

        public void DeleteFile(string isbn)
        {
            File.Delete(GetPath(isbn));
        }
    }
    public class BookCover : AnthologyImage
    {
        public override string GetFolderName()
        {
            return "Book Covers";
        }
    }
    public class AudiobookCover : AnthologyImage
    {
        public override string GetFolderName()
        {
            return "Audiobook Covers";
        }
    }
    public class BookImage : AnthologyImage
    {
        public override string GetFolderName()
        {
            return "Extra Images";
        }
    }
}
