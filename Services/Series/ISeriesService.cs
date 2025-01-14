using Anthology.Data;
using Anthology.Plugins.Models;

namespace Anthology.Services
{
    public interface ISeriesService
    {
        List<Series> GetSeries();
        void RefreshMetadataSeries();
        List<Series> GetAllSeries(Metadata metadata = null);
        List<Series> GetAllSeries(List<Book> books);
        Series GetSeries(Guid guid);
        Series? GetSeries(string name);
        void SaveSeries(Series series, bool newSeries = false);
        void DeleteSeries(Series series);
    }
}