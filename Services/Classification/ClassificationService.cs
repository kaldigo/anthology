using Anthology.Data;
using Anthology.Plugins.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Services
{
    public class ClassificationService : IClassificationService
    {
        DatabaseContext _context;
        private static List<Classification> _metadataClassificationsCache;
        private static Task _refreshMetadataClassificationsTask;
        private static bool _isCached;

        public ClassificationService(DatabaseContext context)
        {
            _context = context;
            _isCached = false;
            _metadataClassificationsCache = new List<Classification>();
        }

        public List<Classification> GetClassifications()
        {
            return _context.Classifications.ToList();
        }
        private static Task<string> RefreshMetadataClassificationsTask(DatabaseContext context, ClassificationService instance)
        {
            if (!(_refreshMetadataClassificationsTask != null && (_refreshMetadataClassificationsTask.Status == TaskStatus.Running || _refreshMetadataClassificationsTask.Status == TaskStatus.WaitingToRun || _refreshMetadataClassificationsTask.Status == TaskStatus.WaitingForActivation)))
            {
                _refreshMetadataClassificationsTask = Task.Factory.StartNew(() =>
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var bookMetadata = context.Books.Select(b => b.BookMetadata).ToList();
                        var metadataGenresStrings = bookMetadata.SelectMany(b => b.Genres).ToList();
                        var metadataGenres = metadataGenresStrings.Select(g => new Classification()
                        { Name = g, Type = Classification.ClassificationType.Genre });

                        var metadataTagsStrings = bookMetadata.SelectMany(b => b.Tags).ToList();
                        var metadataTags = metadataTagsStrings.Select(t => new Classification()
                        { Name = t, Type = Classification.ClassificationType.Tag });

                        var metadataClassifications = instance.CleanClassification(metadataGenres.Concat(metadataTags).ToList());

                        _metadataClassificationsCache = metadataClassifications;
                        _isCached = true;
                    }
                });
            }
            _refreshMetadataClassificationsTask.Wait();
            return Task.FromResult("Refresh complete");
        }

        public void RefreshMetadataClassifications()
        {
            RefreshMetadataClassificationsTask(_context, this);
        }
        public List<Classification> GetAllClassifications(Metadata metadata = null)
        {
            if (_isCached == false) RefreshMetadataClassifications();
            List<Classification> metadataClassifications;
            if (metadata != null)
            {
                var metadataGenres = metadata.Genres.Select(g => new Classification()
                { Name = g, Type = Classification.ClassificationType.Genre });
                
                var metadataTags = metadata.Tags.Select(t => new Classification()
                { Name = t, Type = Classification.ClassificationType.Tag });

                metadataClassifications = CleanClassification(metadataGenres.Concat(metadataTags).Concat(_metadataClassificationsCache).ToList());
            }
            else
            {
                metadataClassifications = _metadataClassificationsCache;
            }

            var dbClassifications = _context.Classifications.ToList();
            var allClassifications = dbClassifications.Concat(metadataClassifications).DistinctBy(c => c.Name)
                .ToList();
            return allClassifications;
        }

        public List<Classification> GetAllClassifications(List<Book> books)
        {
            if (_isCached == false) RefreshMetadataClassifications();

            var bookMetadata = books.Select(b => b.BookMetadata).ToList();

            var metadataGenres = bookMetadata.SelectMany(b => b.Genres).Select(g => new Classification()
            { Name = g, Type = Classification.ClassificationType.Genre });

            var metadataTags = bookMetadata.SelectMany(b => b.Tags).Select(t => new Classification()
            { Name = t, Type = Classification.ClassificationType.Tag });

            var metadataClassifications = CleanClassification(metadataGenres.Concat(metadataTags).Concat(_metadataClassificationsCache).ToList());

            var dbClassifications = _context.Classifications.ToList();
            var allClassifications = dbClassifications.Concat(metadataClassifications).DistinctBy(c => c.Name)
                .ToList();

            return allClassifications;
        }

        public Classification GetClassification(Guid guid)
        {
            return _context.Classifications.First(c => c.ID == guid);
        }

        public Classification? GetClassification(string name)
        {
            return _context.Classifications.FirstOrDefault(c => c.Name.ToLower() == name.ToLower() || c.Aliases.Any(a => a.Name.ToLower() == name.ToLower()));
        }

        public void SaveClassification(Classification classification, bool newClassification = false)
        {
            if (newClassification) _context.Classifications.Add(classification);
            else _context.Classifications.Update(classification);
            _context.SaveChanges();
        }

        public void DeleteClassification(Classification classification)
        {
            _context.Classifications.Remove(classification);
            _context.SaveChanges();
        }

        public List<Classification> CleanClassification(List<Classification> classifications)
        {
            var cleanedClassifications = new List<Classification>();

            foreach (var item in classifications)
            {
                var classification = GetClassification(item.Name);

                if (classification != null) cleanedClassifications.Add(classification);
                else cleanedClassifications.Add(item);
            }

            return cleanedClassifications.DistinctBy(c => c.Name).ToList();
        }
    }
}
