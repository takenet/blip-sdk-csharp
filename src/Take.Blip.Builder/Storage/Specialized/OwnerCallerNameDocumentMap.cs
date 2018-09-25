using System;
using System.Threading.Tasks;
using Take.Blip.Builder.Hosting;
using Take.Elephant.Specialized;

namespace Take.Blip.Builder.Storage.Specialized
{
    public class OwnerCallerNameDocumentMap : OnDemandCacheMap<OwnerCallerName, StorageDocument>, IOwnerCallerNameDocumentMap
    {
        private readonly ISourceOwnerCallerNameDocumentMap _source;
        private readonly ICacheOwnerCallerNameDocumentMap _cache;
        private readonly IConfiguration _configuration;

        public OwnerCallerNameDocumentMap(ISourceOwnerCallerNameDocumentMap source, ICacheOwnerCallerNameDocumentMap cache, IConfiguration configuration)
            : base(source, cache, configuration.OnDemandCacheExpiration)
        {
            _source = source;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task SetRelativeKeyExpirationAsync(OwnerCallerName key, TimeSpan ttl)
        {
            await _source.SetRelativeKeyExpirationAsync(key, ttl);

            if (ttl < _configuration.OnDemandCacheExpiration)
            {
                await _cache.SetRelativeKeyExpirationAsync(key, ttl);
            }
        }

        public async Task SetAbsoluteKeyExpirationAsync(OwnerCallerName key, DateTimeOffset expiration)
        {
            await _source.SetAbsoluteKeyExpirationAsync(key, expiration);

            if ((expiration - DateTimeOffset.UtcNow) < _configuration.OnDemandCacheExpiration)
            {
                await _cache.SetAbsoluteKeyExpirationAsync(key, expiration);
            }
        }
    }
}
