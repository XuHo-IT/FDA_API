using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace FDAAPI.Infra.Services.Cache
{
    public class RedisStateCache : IStateCache
    {
        private readonly IDistributedCache _cache;

        public RedisStateCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetStateAsync(string state, string? returnUrl, TimeSpan expiration, CancellationToken ct = default)
        {
            var key = $"oauth:state:{state}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _cache.SetStringAsync(key, returnUrl ?? string.Empty, options, ct);
        }

        public async Task<string?> GetStateAsync(string state, CancellationToken ct = default)
        {
            var key = $"oauth:state:{state}";
            return await _cache.GetStringAsync(key, ct);
        }

        public async Task RemoveStateAsync(string state, CancellationToken ct = default)
        {
            var key = $"oauth:state:{state}";
            await _cache.RemoveAsync(key, ct);
        }
    }
}
