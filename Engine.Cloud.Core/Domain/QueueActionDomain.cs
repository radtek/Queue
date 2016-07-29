using System.Data.Entity;
using Engine.Cloud.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using RefactorThis.GraphDiff;
using Engine.Cloud.Core.Model.DataContext;

namespace Engine.Cloud.Core.Domain
{
    public class QueueActionDomain
    {
        private readonly EngineCloudDataContext _context;

        public QueueActionDomain(EngineCloudDataContext context)
        {
            _context = context;
        }

        public IEnumerable<QueueAction> GetAll(Expression<Func<QueueAction, bool>> predicate)
        {
            return _context.QueueAction.Where(predicate);
        }

        public QueueAction Get(Expression<Func<QueueAction, bool>> predicate)
        {
            return _context.QueueAction.Where(predicate).FirstOrDefault();
        }

        public void UpdateAction(QueueAction action)
        {
            _context.Entry(action).State = EntityState.Modified;
            
            action.LastUpdate = DateTime.Now;
            
            _context.SaveChanges();
        }

        public void CreateQueueAction(QueueAction queueAction)
        {
            queueAction.LastUpdate = DateTime.Now;
            queueAction.CreateDate = DateTime.Now;

            foreach (var step in queueAction.QueueActionStep)
            {
                step.LastUpdate = DateTime.Now;
            }

            var result = _context.UpdateGraph(queueAction, map => map.OwnedCollection(p => p.QueueActionStep));

            queueAction.QueueActionReference.QueueActionId = result.QueueActionId;

            _context.SaveChanges();

            queueAction.QueueActionReference.QueueActionId = result.QueueActionId;

            AddQueueActionReference(queueAction.QueueActionReference);

            queueAction.QueueActionId = result.QueueActionId;
        }

        private void AddQueueActionReference(QueueActionReference queueActionReference)
        {
            _context.QueueActionReference.Attach(queueActionReference);
            _context.Entry(queueActionReference).State = EntityState.Added;

            _context.SaveChanges();
        }
    }
}