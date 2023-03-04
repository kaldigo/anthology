using Anthology.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace Anthology.Services
{
    public class ImageService : IImageService
    {
        DatabaseContext _context;

        public ImageService(DatabaseContext context)
        {
            _context = context;
        }

        public void DeleteImage(Image image)
        {
            string path = "";
            switch (image.GetCategoryName())
            {
                case "Book":
                    Book book;
                    switch (image.GetSubCategoryName())
                    {
                        case "Book Covers":
                            path = ((BookCover)image).GetPath();
                            book = ((BookCover)image).Book;
                            book.BookCovers.Remove(((BookCover)image));
                            break;
                        case "Audiobook Covers":
                            path = ((AudiobookCover)image).GetPath();
                            book = ((AudiobookCover)image).Book;
                            book.AudiobookCovers.Remove(((AudiobookCover)image));
                            break;
                        case "Extra Images":
                            path = ((BookImage)image).GetPath();
                            book = ((BookImage)image).Book;
                            book.Images.Remove(((BookImage)image));
                            break;
                    }
                    break;
                case "Person":
                    path = ((PersonImage)image).GetPath();
                    var person = ((PersonImage)image).Person;
                    person.Images.Remove(((PersonImage)image));
                    break;
                case "Temp":
                    path = ((TempImage)image).GetPath();
                    break;
            }

            _context.SaveChanges();
            if(!string.IsNullOrWhiteSpace(path)) File.Delete(path);
        }

        public Data.Image GetImage(Guid guid)
        {
            Data.Image? image = _context.Books.SelectMany(b => b.BookCovers).FirstOrDefault(i => i.ID == guid);
            if (image == null) image = _context.Books.SelectMany(b => b.AudiobookCovers).FirstOrDefault(i => i.ID == guid);
            if (image == null) image = _context.Books.SelectMany(b => b.Images).FirstOrDefault(i => i.ID == guid);
            if (image == null) image = _context.People.SelectMany(b => b.Images).FirstOrDefault(i => i.ID == guid);

            return image;
        }

        public Image? GetImage(string category, string id, string filename, string subcategory = null)
        {
            Image? image = null;
            switch (category)
            {
                case "Book":
                    var book = _context.Books.FirstOrDefault(b => b.ISBN == id);
                    switch (subcategory)
                    {
                        case "BookCovers":
                            if(book != null) image = book.BookCovers.FirstOrDefault(i => i.FileName == filename);
                            break;
                        case "AudiobookCovers":
                            if (book != null) image = book.AudiobookCovers.FirstOrDefault(i => i.FileName == filename);
                            break;
                        case "ExtraImages":
                            if (book != null) image = book.Images.FirstOrDefault(i => i.FileName == filename);
                            break;
                    }
                    break;
                case "Person":
                    var person = _context.People.FirstOrDefault(p => p.ID.ToString() == id);
                    if (person != null) image = person.Images.FirstOrDefault(i => i.FileName == filename);
                    break;
                case "Temp":
                    image = new TempImage() { FileName = filename, TempPath = Guid.Parse(id) };
                    break;
            }
            return image;
        }

        public async Task<TempImage> SaveTempImage(IBrowserFile image)
        {
            var tempImage = new TempImage() { FileName = image.Name};
            var path = tempImage.GetPath();
            var directory = Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);
            await using FileStream fs = new(path, FileMode.Create);
            await image.OpenReadStream(512000000 ).CopyToAsync(fs);
            return tempImage;
        }
    }
}
