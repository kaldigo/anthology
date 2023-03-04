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
        public bool IsPrimary { get; set; } = false;
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
            List<string> path = new List<string>();
            path.Add("/Images");
            path.Add(GetCategoryName().Replace(" ", ""));
            path.Add(id);
            if (GetSubCategoryName() != "") path.Add(GetSubCategoryName().Replace(" ", ""));
            path.Add(FileName);

            return string.Join("/", path.ToArray());
        }

        public string GetPath()
        {
            return GetPath(GetID());
        }

        public string GetPath(string id)
        {
            List<string> path = new List<string>();
            if (GetCategoryName() == "Temp")
            {
                path.Add(Utils.FileUtils.GetTempPath());
                path.Add("Media");
            }
            else path.Add(Utils.FileUtils.GetMediaPath());
            path.Add("Images");
            if (GetCategoryName() != "Temp") path.Add(GetCategoryName());
            path.Add(id);
            if (GetSubCategoryName() != "") path.Add(GetSubCategoryName());
            path.Add(FileName);

            return Path.Combine(path.ToArray());
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
    public class TempImage : Image
    {
        public Guid TempPath { get; set; } = Guid.NewGuid();
        public override string GetCategoryName()
        {
            return "Temp";
        }
        public override string GetSubCategoryName()
        {
            return "";
        }
        public override string GetID()
        {
            return TempPath.ToString();
        }
    }
}
