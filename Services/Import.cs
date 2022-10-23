using Anthology.Data.DB;
using Anthology.Utils;

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
                    var context = new DatabaseContext();
                    var books = Readarr.GetBooks();

                    var booksToAdd = new List<Book>();
                    foreach (var book in books)
                    {
                        if(!context.Books.Any(b => b.GRID == book.foreignBookId))
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
                });

                _task.Wait();

                return Task.FromResult("Import complete");
            }
        }
    }
}
