using Anthology.Data;
using Anthology.Plugins.Models;

namespace Anthology.Services
{
    public interface IClassificationService
    {
        void DeleteClassification(Classification classification);
        Classification GetClassification(Guid guid);
        Classification? GetClassification(string name);
        List<Classification> GetClassifications();
        List<Classification> GetAllClassifications(Metadata metadata = null);
        void SaveClassification(Classification classification, bool newClassification = false);
        List<Classification> CleanClassification(List<Classification> classifications);

    }
}