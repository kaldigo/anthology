using Anthology.Data;

namespace Anthology.Services
{
    public interface IClassificationService
    {
        void DeleteClassification(Classification classification);
        Classification GetClassification(Guid guid);
        Classification? GetClassification(string name);
        List<Classification> GetClassifications();
        void SaveClassification(Classification classification, bool newClassification = false);
        List<Classification> CleanClassification(List<Classification> classifications);

    }
}