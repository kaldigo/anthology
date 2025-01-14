using Anthology.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anthology.Plugins.Models;

namespace Anthology.Services
{
    public class SeriesService : ISeriesService
    {
        DatabaseContext _context;
        IClassificationService _classificationService;
        private static List<Series> _metadataSeriesCache;
        private static Task _refreshMetadataSeriesTask;
        private static bool _isCached;

        public SeriesService(DatabaseContext context, IClassificationService classificationService)
        {
            _context = context;
            _classificationService = classificationService;
        }

        public List<Series> GetSeries()
        {
            return _context.Series.ToList();
        }
        private static Task<string> RefreshMetadataSeriesTask(DatabaseContext context)
        {
            if (!(_refreshMetadataSeriesTask != null && (_refreshMetadataSeriesTask.Status == TaskStatus.Running || _refreshMetadataSeriesTask.Status == TaskStatus.WaitingToRun || _refreshMetadataSeriesTask.Status == TaskStatus.WaitingForActivation)))
            {
                _refreshMetadataSeriesTask = Task.Factory.StartNew(() =>
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var seriesList = context.Series.ToList();
                        var bookMetadata = context.Books.Select(b => b.BookMetadata).ToList();
                        var metadataSeriesList = bookMetadata.SelectMany(b => b.Series).Select(m => new Series() { Name = m.Name }).ToList();

                        var cleanedMetadataSeriesList = new List<Series>();
                        foreach (var metadataSeries in metadataSeriesList)
                        {
                            var series = seriesList.FirstOrDefault(s =>
                                s.Name == metadataSeries.Name || s.Aliases.Any(a => a.Name == metadataSeries.Name));
                            if (series == null) cleanedMetadataSeriesList.Add(metadataSeries);
                        }

                        _metadataSeriesCache = cleanedMetadataSeriesList.DistinctBy(s => s.Name).ToList();
                        _isCached = true;
                    }
                });
            }
            _refreshMetadataSeriesTask.Wait();
            return Task.FromResult("Refresh complete");
        }

        public void RefreshMetadataSeries()
        {
            RefreshMetadataSeriesTask(_context);
        }

        public List<Series> GetAllSeries(Metadata metadata = null)
        {
            if (_isCached == false) RefreshMetadataSeries();
            var seriesList = _context.Series.ToList();
            seriesList = seriesList.Concat(_metadataSeriesCache).ToList();
            if (metadata != null)
            {
                foreach (var metadataSeries in metadata.Series)
                {
                    var series = seriesList.FirstOrDefault(s =>
                        s.Name == metadataSeries.Name || s.Aliases.Any(a => a.Name == metadataSeries.Name));
                    if (series == null) seriesList.Add(new Series() { Name = metadataSeries.Name });
                }
            }

            return seriesList;
        }

        public List<Series> GetAllSeries(List<Book> books)
        {
            if (_isCached == false) RefreshMetadataSeries();

            var bookMetadata = books.Select(b => b.BookMetadata).ToList();

            var metadataSeriesList = bookMetadata.SelectMany(b => b.Series).Select(m => new Series() { Name = m.Name }).ToList();

            var seriesList = _context.Series.ToList();
            seriesList = seriesList.Concat(_metadataSeriesCache).ToList();

            foreach (var metadataSeries in metadataSeriesList)
            {
                var series = seriesList.FirstOrDefault(s =>
                    s.Name == metadataSeries.Name || s.Aliases.Any(a => a.Name == metadataSeries.Name));
                if (series == null) seriesList.Add(new Series() { Name = metadataSeries.Name });
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
            if (series == null) series = new Series(name);
            series.BookClassifications = _classificationService.CleanClassification(series.BookClassificationsRaw);
            return series;
        }

        public void SaveSeries(Series series, bool newSeries = false)
        {
            if (newSeries) _context.Series.Add(series);
            else _context.Series.Update(series);
            _context.SaveChanges();
        }

        public void DeleteSeries(Series series)
        {
            _context.Series.Remove(series);
            _context.SaveChanges();
        }
    }
}
