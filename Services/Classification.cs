using Anthology.Data.DB;
using Microsoft.EntityFrameworkCore;

namespace Anthology.Services
{
    public interface IClassificationService
    {
        List<Classification> GetClassifications();
        Classification GetClassificationByID(Guid id);
        void SaveClassification(Classification classification);
        void DeleteClassification(Guid id);
    }
    public class ClassificationService : IClassificationService
    {
        private static readonly DatabaseContext _dbContext = new DatabaseContext();
        public List<Classification> GetClassifications()
        {
            return _dbContext.Classifications.ToList();
        }
        public Classification GetClassificationByID(Guid id)
        {
            var classification = _dbContext.Classifications.SingleOrDefault(x => x.ID == id);
            return classification;
        }
        public void SaveClassification(Classification classification)
        {
            if (classification.ID == null)
            {
                classification.ID = new Guid();
                _dbContext.Classifications.Add(classification);
            }
            else _dbContext.Classifications.Update(classification);
            _dbContext.SaveChanges();
        }
        public void DeleteClassification(Guid id)
        {
            var classification = _dbContext.Classifications.FirstOrDefault(x => x.ID == id);
            if (classification != null)
            {
                _dbContext.Classifications.Remove(classification);
                _dbContext.SaveChanges();
            }
        }
    }
}
