using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tweetbook.Services
{
    public interface IResponseCacheService
    {
        Task CacheResponseAsync(string cacheKay, object response, TimeSpan timeTimeLive);
        Task<string> GetCachedResponseAsync(string cacheKey);
    }
}
