using Anthology.Data;
using Anthology.Plugins.Models;

namespace Anthology.Services
{
    public interface ISeriesService
    {
        List<Series> GetSeries();
        List<Series> GetAllSeries(Metadata metadata = null);
        Series GetSeries(Guid guid);
        Series? GetSeries(string name);
        void SaveSeries(Series series, bool newSeries = false);
        void DeleteSeries(Series series);
    }
}