using Anthology.Data;
using Anthology.Plugins.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Services
{
    public class ClassificationService : IClassificationService
    {
        private readonly DatabaseContext _context;

        public ClassificationService(DatabaseContext context)
        {
            _context = context;
        }

        public List<Classification> GetClassifications()
        {
            return _context.Classifications.ToList();
        }

        /// <summary>
        /// Fetches all metadata classifications (genres + tags from BookMetadata) 
        /// and caches them in MemoryCache if not already cached.
        /// </summary>
        public void RefreshMetadataClassifications()
        {
            // Access the default memory cache
            MemoryCache cache = MemoryCache.Default;
            const string cacheKey = "MetadataClassifications";

            // If we already have it in cache, just return
            if (cache.Contains(cacheKey))
            {
                return;
            }

            // Otherwise, fetch from the database
            var bookMetadata = _context.Books
                .Select(b => b.BookMetadata)
                .ToList();

            var metadataGenresStrings = bookMetadata
                .SelectMany(b => b.Genres)
                .ToList();

            var metadataTagsStrings = bookMetadata
                .SelectMany(b => b.Tags)
                .ToList();

            var metadataGenres = metadataGenresStrings
                .Select(g => new Classification
                {
                    Name = g,
                    Type = Classification.ClassificationType.Genre
                });

            var metadataTags = metadataTagsStrings
                .Select(t => new Classification
                {
                    Name = t,
                    Type = Classification.ClassificationType.Tag
                });

            // Clean up duplicates or unify references
            var metadataClassifications = CleanClassification(
                metadataGenres.Concat(metadataTags).ToList()
            );

            // Store it in the cache with an expiration policy (adjust as needed)
            cache.Set(
                cacheKey,
                metadataClassifications,
                new CacheItemPolicy
                {
                    // Example: keep the item in cache for 30 minutes
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30)
                }
            );
        }

        /// <summary>
        /// Gets all classifications from the database and merges them 
        /// with the metadata classifications from the cache.
        /// </summary>
        public List<Classification> GetAllClassifications(Metadata metadata = null)
        {
            // Make sure the cache is populated
            RefreshMetadataClassifications();

            // Retrieve from memory cache
            var cache = MemoryCache.Default;
            var cachedValue = cache.Get("MetadataClassifications") as List<Classification>;
            if (cachedValue == null) // If for some reason it isn't there, re-fetch
            {
                RefreshMetadataClassifications();
                cachedValue = cache.Get("MetadataClassifications") as List<Classification>;
            }

            // If the consumer provided Metadata, combine it with cached classifications
            List<Classification> metadataClassifications;
            if (metadata != null)
            {
                var metadataGenres = metadata.Genres.Select(g => new Classification
                {
                    Name = g,
                    Type = Classification.ClassificationType.Genre
                });

                var metadataTags = metadata.Tags.Select(t => new Classification
                {
                    Name = t,
                    Type = Classification.ClassificationType.Tag
                });

                // Merge new metadata items with cached items
                metadataClassifications = CleanClassification(
                    metadataGenres
                        .Concat(metadataTags)
                        .Concat(cachedValue ?? new List<Classification>())
                        .ToList()
                );
            }
            else
            {
                metadataClassifications = cachedValue ?? new List<Classification>();
            }

            // Fetch DB classifications
            var dbClassifications = _context.Classifications.ToList();

            // Merge DB with metadata-based classifications
            var allClassifications = dbClassifications
                .Concat(metadataClassifications)
                .DistinctBy(c => c.Name)
                .ToList();

            return allClassifications;
        }

        public List<Classification> GetAllClassifications(List<Book> books)
        {
            // Make sure the cache is populated
            RefreshMetadataClassifications();

            // Retrieve from memory cache
            var cache = MemoryCache.Default;
            var cachedValue = cache.Get("MetadataClassifications") as List<Classification>;
            if (cachedValue == null)
            {
                RefreshMetadataClassifications();
                cachedValue = cache.Get("MetadataClassifications") as List<Classification>;
            }

            var bookMetadata = books.Select(b => b.BookMetadata).ToList();

            var metadataGenres = bookMetadata
                .SelectMany(b => b.Genres)
                .Select(g => new Classification
                {
                    Name = g,
                    Type = Classification.ClassificationType.Genre
                });

            var metadataTags = bookMetadata
                .SelectMany(b => b.Tags)
                .Select(t => new Classification
                {
                    Name = t,
                    Type = Classification.ClassificationType.Tag
                });

            // Merge new book metadata with cached items
            var metadataClassifications = CleanClassification(
                metadataGenres
                    .Concat(metadataTags)
                    .Concat(cachedValue ?? new List<Classification>())
                    .ToList()
            );

            // Combine with DB classifications
            var dbClassifications = _context.Classifications.ToList();
            var allClassifications = dbClassifications
                .Concat(metadataClassifications)
                .DistinctBy(c => c.Name)
                .ToList();

            return allClassifications;
        }

        public Classification GetClassification(Guid guid)
        {
            return _context.Classifications.First(c => c.ID == guid);
        }

        public Classification? GetClassification(string name)
        {
            return _context.Classifications
                .FirstOrDefault(
                    c => c.Name.ToLower() == name.ToLower() ||
                         c.Aliases.Any(a => a.Name.ToLower() == name.ToLower())
                );
        }

        public void SaveClassification(Classification classification, bool newClassification = false)
        {
            if (newClassification)
            {
                _context.Classifications.Add(classification);
            }
            else
            {
                _context.Classifications.Update(classification);
            }

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

                if (classification != null)
                {
                    cleanedClassifications.Add(classification);
                }
                else
                {
                    cleanedClassifications.Add(item);
                }
            }

            return cleanedClassifications.DistinctBy(c => c.Name).ToList();
        }
    }
}
