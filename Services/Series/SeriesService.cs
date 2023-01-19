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

        public SeriesService(DatabaseContext context, IClassificationService classificationService)
        {
            _context = context;
            _classificationService = classificationService;
        }

        public List<Series> GetSeries()
        {
            return _context.Series.ToList();
        }

        public List<Series> GetAllSeries(Metadata metadata = null)
        {
            var seriesList = _context.Series.ToList();
            var bookMetadata = _context.Books.Select(b => b.BookMetadata).ToList();
            if(metadata != null) bookMetadata.Add(metadata);
            var metadataSeriesList = bookMetadata.SelectMany(b => b.Series).ToList();
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
