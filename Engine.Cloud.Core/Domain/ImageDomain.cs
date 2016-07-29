using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.DataContext;

namespace Engine.Cloud.Core.Domain
{
    public class ImageDomain
    {
        private readonly EngineCloudDataContext _context;

        public ImageDomain(EngineCloudDataContext context)
        {
            _context = context;
        }

        public Image Get(Expression<Func<Image, bool>> predicate)
        {
            return _context.Image.Where(predicate).FirstOrDefault();
        }

        public IEnumerable<Image> GetAll(Expression<Func<Image, bool>> predicate)
        {
            return _context.Image.Where(predicate);
        }

        public IQueryable<Image> GetAll()
        {
            return _context.Image;
        }

        public void AddUpdateImage(Image image)
        {
            _context.Image.Attach(image);

            _context.Entry(image).State = image.ImageId == 0
                ? EntityState.Added
                : EntityState.Modified;

            _context.SaveChanges();
        }
    }
}

