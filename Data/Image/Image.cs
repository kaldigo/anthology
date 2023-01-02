using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anthology.Data
{
    public abstract class Image
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ID { get; set; } = new Guid();
        public string FileName { get; set; }
        public Image() { }
        public Image(string fileName)
        {
            this.FileName = fileName;
        }

        public abstract string GetCategoryName();
        public abstract string GetSubCategoryName();
        public abstract string GetID();

        public string GetUrl()
        {
            return GetUrl(GetID());
        }
        public string GetUrl(string id)
        {
            string[] path = new string[] { };
            path.Append(Environment.GetEnvironmentVariable("ANTHOLOGY_URL"));
            path.Append("api/Image");
            path.Append(GetCategoryName().Replace(" ", ""));
            path.Append(id);
            if (GetSubCategoryName() != "") path.Append(GetSubCategoryName().Replace(" ", ""));
            path.Append(FileName);

            return string.Join("/", path);
        }

        public string GetPath()
        {
            return GetPath(GetID());
        }

        public string GetPath(string id)
        {
            string[] path = new string[] { };
            path.Append(Utils.FileUtils.GetConfigPath());
            path.Append("Images");
            path.Append(GetCategoryName());
            path.Append(id);
            if(GetSubCategoryName() != "") path.Append(GetSubCategoryName());
            path.Append(FileName);

            return Path.Combine(path);
        }

        public void DeleteFile()
        {
            DeleteFile(GetID());
        }

        public void DeleteFile(string id)
        {
            File.Delete(GetPath(id));
        }
    }
    public class BookCover : Image
    {
        public virtual Book Book { get; set; }
        public override string GetCategoryName()
        {
            return "Book";
        }
        public override string GetSubCategoryName()
        {
            return "Book Covers";
        }
        public override string GetID()
        {
            return Book.ISBN;
        }
    }
    public class AudiobookCover : Image
    {
        public virtual Book Book { get; set; }
        public override string GetCategoryName()
        {
            return "Book";
        }
        public override string GetSubCategoryName()
        {
            return "Audiobook Covers";
        }
        public override string GetID()
        {
            return Book.ISBN;
        }
    }
    public class BookImage : Image
    {
        public virtual Book Book { get; set; }
        public override string GetCategoryName()
        {
            return "Book";
        }
        public override string GetSubCategoryName()
        {
            return "Extra Images";
        }
        public override string GetID()
        {
            return Book.ISBN;
        }
    }
    public class PersonImage : Image
    {
        public virtual Person Person { get; set; }
        public override string GetCategoryName()
        {
            return "Person";
        }
        public override string GetSubCategoryName()
        {
            return "";
        }
        public override string GetID()
        {
            return Person.ID.ToString();
        }
    }
}
