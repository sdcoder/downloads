using FirstAgain.Common.Logging;
using HttpCache.Abstractions;
using HttpCache.Extensions;
using Soss.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LightStreamWeb.Caching
{
    public sealed class ScaleOutHttpCache : IHttpCache<string>
    {
        private static readonly ScaleOutHttpCache instance = new ScaleOutHttpCache();

        private readonly NamedCache cache;
        private readonly int ttlOverrideInSeconds;

        public static ScaleOutHttpCache Instance
        {
            get
            {
                return instance;
            }
        }

        public IHttpCacheOptions Options { get; private set; }

        static ScaleOutHttpCache() { }

        private ScaleOutHttpCache()
        {
            cache = CacheFactory.GetCache("Kentico");
            cache.AllowClientCaching = true;
            cache.DefaultCreatePolicy = new CreatePolicy(0); // using 0 for infinite timeout

            Int32.TryParse(ConfigurationManager.AppSettings["HttpCache:TtlOverrideInSeconds"], out ttlOverrideInSeconds);

            Options = new HttpCacheOptions()
            {
                TtlOverridesInSeconds = new Dictionary<string, double>
                {
                    { "text/html", ttlOverrideInSeconds},
                    { "application/x-javascript", ttlOverrideInSeconds},
                }
            };
        }

        public bool Contains(string key)
        {
            var containsItem = cache.Contains(key);

            return containsItem;
        }

        public async Task<IHttpCacheItem> GetAsync(string key)
        {
            var cachedItem = cache.Get(key) as byte[];

            IHttpCacheItem item = null;

            if (cachedItem != null)
                item = cachedItem.ToProxyCacheItem();

            return await Task.FromResult(item);
        }

        public Task SaveAsync(string key, IHttpCacheItem item)
        {
            if (!cache.Contains(key))
                cache.Add(key, item.ToByteArray());
            else
            {
                var origItem = (cache.Get(key) as byte[]).ToProxyCacheItem();
                cache.Update(key, item.ToByteArray(), true);
            }

            return Task.CompletedTask;
        }

        public Task DeleteAllAsync()
        {
            cache.Clear();

            return Task.CompletedTask;
        }

        public Task DeleteAsync(string key)
        {
            if (cache.Contains(key))
                cache.Remove(key);

            return Task.CompletedTask;
        }
    }
}
