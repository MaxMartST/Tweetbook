using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Tweetbook.Services
{
    public class ResponseCacheService : IResponseCacheService
    {
        private readonly IDistributedCache _distributedCache;

        public ResponseCacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task CacheResponseAsync(string cacheKay, object response, TimeSpan timeTimeLive)
        {
            if (response is null)
            {
                return;
            }

            var serializedResponse = JsonConvert.SerializeObject(response);

            await _distributedCache.SetStringAsync(cacheKay, serializedResponse, new DistributedCacheEntryOptions
            { 
                AbsoluteExpirationRelativeToNow = timeTimeLive
            });
        }

        public async Task<string> GetCachedResponseAsync(string cacheKey)
        {
            var cachedResponse = await _distributedCache.GetStringAsync(cacheKey);
            return string.IsNullOrEmpty(cachedResponse) ? null : cachedResponse;
        }
    }
}
