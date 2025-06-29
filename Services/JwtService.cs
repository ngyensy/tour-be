using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;
using System;

namespace WebApi.Services
{
    public class JwtService
    {
        private readonly IDistributedCache _cache;
        private readonly TimeSpan _expiration = TimeSpan.FromMinutes(30);

        public JwtService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            var tokenFromCache = await _cache.GetStringAsync(token);
            return !string.IsNullOrEmpty(tokenFromCache);
        }

        public async Task StoreTokenAsync(string token)
        {
            await _cache.SetStringAsync(token, "valid", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _expiration
            });
        }

        public async Task InvalidateTokenAsync(string token)
        {
            await _cache.RemoveAsync(token);
        }

    }
}
