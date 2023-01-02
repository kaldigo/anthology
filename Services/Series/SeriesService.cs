using Anthology.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Series GetSeries(Guid guid)
        {
            var series = _context.Series.First(c => c.ID == guid);
            series.BookClassifications = _classificationService.CleanClassification(series.BookClassificationsRaw);
            return series;
        }

        public Series? GetSeries(string name)
        {
            var series = _context.Series.FirstOrDefault(c => c.Name == name);
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
