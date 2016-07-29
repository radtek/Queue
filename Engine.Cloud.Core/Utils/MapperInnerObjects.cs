using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils
{
    public class MapperInnerObjects<TSource, TDest> where TDest : new()
    {
        protected virtual void CopyMatchingProperties(TSource source, TDest dest)
        {
            foreach (var destProp in typeof(TDest).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite))
            {
                var sourceProp =
                    typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.Name == destProp.Name && p.PropertyType == destProp.PropertyType).
                        FirstOrDefault();
                if (sourceProp != null)
                {
                    destProp.SetValue(dest, sourceProp.GetValue(source, null), null);
                }
            }
        }
        protected readonly IList<Action<TSource, TDest>> mappings = new List<Action<TSource, TDest>>();

        public virtual void AddMapping(Action<TSource, TDest> mapping)
        {
            mappings.Add(mapping);
        }

        public virtual TDest MapObject(TSource source, TDest dest)
        {
            CopyMatchingProperties(source, dest);
            foreach (var action in mappings)
            {
                action(source, dest);
            }

            return dest;
        }

        public virtual TDest CreateMappedObject(TSource source)
        {
            TDest dest = new TDest();
            return MapObject(source, dest);
        }
    }
}
