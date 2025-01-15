using Anthology.Data;
using Anthology.Plugins.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching; // For MemoryCache
using System.Threading.Tasks;

namespace Anthology.Services
{
    public class SeriesService : ISeriesService
    {
        private readonly DatabaseContext _context;
        private readonly IClassificationService _classificationService;

        public SeriesService(DatabaseContext context, IClassificationService classificationService)
        {
            _context = context;
            _classificationService = classificationService;
        }

        public List<Series> GetSeries()
        {
            return _context.Series.ToList();
        }

        /// <summary>
        /// Refreshes the metadata series and caches the result in MemoryCache.
        /// </summary>
        public void RefreshMetadataSeries()
        {
            // Access the default memory cache
            MemoryCache cache = MemoryCache.Default;
            const string cacheKey = "MetadataSeries";

            // If the cache is already populated, return
            if (cache.Contains(cacheKey))
            {
                return;
            }

            // Otherwise, fetch data and populate the cache
            var seriesList = _context.Series.ToList();
            var bookMetadata = _context.Books.Select(b => b.BookMetadata).ToList();
            var metadataSeriesList = bookMetadata.SelectMany(b => b.Series).Select(m => new Series
            {
                Name = m.Name
            }).ToList();

            var cleanedMetadataSeriesList = new List<Series>();
            foreach (var metadataSeries in metadataSeriesList)
            {
                var series = seriesList.FirstOrDefault(s =>
                    s.Name == metadataSeries.Name || s.Aliases.Any(a => a.Name == metadataSeries.Name));
                if (series == null)
                {
                    cleanedMetadataSeriesList.Add(metadataSeries);
                }
            }

            // Deduplicate and store in cache
            cache.Set(
                cacheKey,
                cleanedMetadataSeriesList.DistinctBy(s => s.Name).ToList(),
                new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30) // Cache for 30 minutes
                }
            );
        }

        /// <summary>
        /// Gets all series, combining metadata series (from cache) and database series.
        /// </summary>
        public List<Series> GetAllSeries(Metadata metadata = null)
        {
            // Ensure the cache is populated
            RefreshMetadataSeries();

            // Retrieve from cache
            var cache = MemoryCache.Default;
            var cachedValue = cache.Get("MetadataSeries") as List<Series>;
            if (cachedValue == null) // If for some reason cache is empty
            {
                RefreshMetadataSeries();
                cachedValue = cache.Get("MetadataSeries") as List<Series>;
            }

            // Combine with database series
            var seriesList = _context.Series.ToList();
            seriesList = seriesList.Concat(cachedValue ?? new List<Series>()).ToList();

            if (metadata != null)
            {
                foreach (var metadataSeries in metadata.Series)
                {
                    var series = seriesList.FirstOrDefault(s =>
                        s.Name == metadataSeries.Name || s.Aliases.Any(a => a.Name == metadataSeries.Name));
                    if (series == null)
                    {
                        seriesList.Add(new Series
                        {
                            Name = metadataSeries.Name
                        });
                    }
                }
            }

            return seriesList;
        }

        public List<Series> GetAllSeries(List<Book> books)
        {
            // Ensure the cache is populated
            RefreshMetadataSeries();

            // Retrieve from cache
            var cache = MemoryCache.Default;
            var cachedValue = cache.Get("MetadataSeries") as List<Series>;
            if (cachedValue == null)
            {
                RefreshMetadataSeries();
                cachedValue = cache.Get("MetadataSeries") as List<Series>;
            }

            // Combine with book metadata
            var bookMetadata = books.Select(b => b.BookMetadata).ToList();
            var metadataSeriesList = bookMetadata.SelectMany(b => b.Series).Select(m => new Series
            {
                Name = m.Name
            }).ToList();

            var seriesList = _context.Series.ToList();
            seriesList = seriesList.Concat(cachedValue ?? new List<Series>()).ToList();

            foreach (var metadataSeries in metadataSeriesList)
            {
                var series = seriesList.FirstOrDefault(s =>
                    s.Name == metadataSeries.Name || s.Aliases.Any(a => a.Name == metadataSeries.Name));
                if (series == null)
                {
                    seriesList.Add(new Series
                    {
                        Name = metadataSeries.Name
                    });
                }
            }

            return seriesList;
        }

        public Series GetSeries(Guid guid)
        {
            var series = _context.Series.First(c => c.ID == guid);
            series.BookClassifications = _classificationService.CleanClassification(series.BookClassificationsRaw);
            return series;
        }

        public Series? GetSeries(string name)
        {
            var series = _context.Series.FirstOrDefault(c => c.Name == name);
            if (series == null)
            {
                series = new Series(name);
            }
            series.BookClassifications = _classificationService.CleanClassification(series.BookClassificationsRaw);
            return series;
        }

        public void SaveSeries(Series series, bool newSeries = false)
        {
            if (newSeries)
            {
                _context.Series.Add(series);
            }
            else
            {
                _context.Series.Update(series);
            }
            _context.SaveChanges();
        }

        public void DeleteSeries(Series series)
        {
            _context.Series.Remove(series);
            _context.SaveChanges();
        }
    }
}
