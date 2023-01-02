using Anthology.Data;

namespace Anthology.Services
{
    public interface ISeriesService
    {
        List<Series> GetSeries();
        Series GetSeries(Guid guid);
        Series? GetSeries(string name);
        void SaveSeries(Series series, bool newSeries = false);
        void DeleteSeries(Series series);
    }
}