using System;
using System.Runtime.Caching;

namespace Engine.Cloud.Panel.Utils
{
    public static class MemoryCacheManager
    {
        public static readonly ObjectCache DefaultMemoryCache = MemoryCache.Default;

        private static readonly CacheEntryUpdateCallback CacheEntryUpdateCallback = UpdatedCallback;

        public static void AddOrUpdate(string key, object cacheItem, double slidingExpiration = 0, double absoluteExpiration = 0)
        {
            var cacheItemPolicy = new CacheItemPolicy();

            if (absoluteExpiration > 0)
                cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddMinutes(absoluteExpiration);
            if (slidingExpiration > 0)
                cacheItemPolicy.SlidingExpiration = TimeSpan.FromMinutes(slidingExpiration);

            cacheItemPolicy.UpdateCallback = CacheEntryUpdateCallback;

            DefaultMemoryCache.Set(key, cacheItem, cacheItemPolicy);
        }

        private static void UpdatedCallback(CacheEntryUpdateArguments arguments)
        {
            //if (arguments.RemovedReason == CacheEntryRemovedReason.Expired)
            //{
            //    var _logger = LogFactory.GetLogger();
            //    _logger.Log(String.Format("MemoryCacheManager: Item Key {0} foi removido do cache.", arguments.Key));
            //}
        }

        public static void Remove(string key)
        {
            DefaultMemoryCache.Remove(key);
        }

        public static object Get(string key)
        {
            var result = DefaultMemoryCache.GetCacheItem(key);
            return result == null ? null : DefaultMemoryCache.GetCacheItem(key).Value;
        }

        public static bool Exists(string key)
        {
            return DefaultMemoryCache.GetCacheItem(key) != null;
        }

    }
}
