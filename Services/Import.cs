using Anthology.Data.DB;
using Anthology.Utils;
using System.Text.RegularExpressions;

namespace Anthology.Services
{
    public class ImportService
    {
        private static Task _task;
        public static Task<string> Run()
        {
            if (_task != null && (_task.Status == TaskStatus.Running || _task.Status == TaskStatus.WaitingToRun || _task.Status == TaskStatus.WaitingForActivation))
            {
                return Task.FromResult("Import already running");
            }
            else
            {
                _task = Task.Factory.StartNew(() =>
                {
                    ImportReadarr();
                    ImportAudiobookshelf();
                });

                _task.Wait();

                return Task.FromResult("Import complete");
            }
        }

        private static void ImportReadarr()
        {
            var context = new DatabaseContext();
            var books = Readarr.GetBooks();

            var booksToAdd = new List<Book>();
            foreach (var book in books)
            {
                if (!context.Books.Any(b => b.GRID == book.foreignBookId))
                {
                    booksToAdd.Add(new Book(book.title, book.author.authorName, book.foreignBookId, context));
                }
            }

            context.Books.AddRange(booksToAdd);
            context.SaveChanges();
            foreach (var book in booksToAdd)
            {
                MetadataService.RefreshBookMetadata(book, context);
            }
        }

        private static void ImportAudiobookshelf()
        {
            var context = new DatabaseContext();
            var books = new AudiobookShelfService().GetMissingBooks(context.Books.Select(b => b.ISBN));

            var booksToAdd = new List<Book>();
            foreach (var book in books)
            {
                var title = Regex.Replace(book.media.metadata.title, @" ?\[.*?\]", string.Empty).Trim();
                booksToAdd.Add(new Book(title, book.media.metadata.authors.Select(a => a.name).ToList(), null, context));
            }

            context.Books.AddRange(booksToAdd);
            context.SaveChanges();
        }
    }
}
