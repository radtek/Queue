using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.DataContext;

namespace Engine.Cloud.Core.Domain
{
    public class QueueActionStepDomain
    {
        private readonly EngineCloudDataContext _context;

        public QueueActionStepDomain(EngineCloudDataContext context)
        {
            _context = context;

        }
        public QueueActionStep Get(Expression<Func<QueueActionStep, bool>> predicate)
        {
            return _context.QueueActionStep.Where(predicate).FirstOrDefault();
        }

        public IEnumerable<QueueActionStep> GetAll(Expression<Func<QueueActionStep, bool>> predicate)
        {
            return _context.QueueActionStep.Where(predicate);
        }

        public void AddUpdateQueueActionStep(QueueActionStep step)
        {
            step.LastUpdate = DateTime.Now;

            _context.QueueActionStep.Attach(step);

            _context.Entry(step).State = step.QueueActionStepId == 0
                ? EntityState.Added
                : EntityState.Modified;

            _context.SaveChanges();
        }
    }
}
