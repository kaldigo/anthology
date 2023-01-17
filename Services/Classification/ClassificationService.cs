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

        public ClassificationService(DatabaseContext context)
        {
            _context = context;
        }

        public List<Classification> GetClassifications()
        {
            return _context.Classifications.ToList();
        }
        public List<Classification> GetAllClassifications(Metadata metadata = null)
        {
            var bookMetadata = _context.Books.Select(b => b.BookMetadata).ToList();
            if(metadata != null) bookMetadata.Add(metadata);
            var metadataGenresStrings = bookMetadata.SelectMany(b => b.Genres).ToList();
            var metadataGenres = metadataGenresStrings.Select(g => new Classification()
                { Name = g, Type = Classification.ClassificationType.Genre });

            var metadataTagsStrings = bookMetadata.SelectMany(b => b.Tags).ToList();
            var metadataTags = metadataTagsStrings.Select(t => new Classification()
                { Name = t, Type = Classification.ClassificationType.Tag });

            var metadataClassifications = CleanClassification(metadataGenres.Concat(metadataTags).ToList());

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
