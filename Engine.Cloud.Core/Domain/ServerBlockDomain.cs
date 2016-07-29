using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.DataContext;

namespace Engine.Cloud.Core.Domain
{
    public class ServerBlockDomain
    {
        private readonly EngineCloudDataContext _context;

        public ServerBlockDomain(EngineCloudDataContext context)
        {
            _context = context;
        }

        public IEnumerable<ServerBlock> GetAll(Expression<Func<ServerBlock, bool>> predicate)
        {
            return _context.ServerBlock.Where(predicate);
        }

        public ServerBlock Get(Expression<Func<ServerBlock, bool>> predicate)
        {
            return _context.ServerBlock.Where(predicate).FirstOrDefault();
        }

        public void AddUpdateServer(ServerBlock serverBlock)
        {
            _context.ServerBlock.Attach(serverBlock);

            _context.Entry(serverBlock).State = serverBlock.ServerBlockId == 0
                ? EntityState.Added
                : EntityState.Modified;

            serverBlock.LastUpdateDate = DateTime.Now;
            if (_context.Entry(serverBlock).State == EntityState.Added)
                serverBlock.CreateDate = DateTime.Now;

            _context.SaveChanges();
        }
    }
}