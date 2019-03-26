using System;
using Lime.Messaging.Resources;
using Microsoft.Extensions.Caching.Memory;

namespace Take.Blip.Builder.Utils
{
    public class ContactMomoryCache : ICache<Contact>
    {
        private readonly MemoryCache _contactCache;
        private readonly TimeSpan _cacheExpiration;

        public ContactMomoryCache(
            TimeSpan cacheExpiration = default(TimeSpan)
            )
        {
            _contactCache = new MemoryCache(new MemoryCacheOptions());
            _cacheExpiration = cacheExpiration == default(TimeSpan) ? TimeSpan.FromMinutes(30) : cacheExpiration;
        }

        public Contact Get(object key) => _contactCache.Get(key) as Contact;

        public void Remove(object key)
        {
            throw new NotImplementedException();
        }

        public void Set(object key, Contact value)
        {
            _contactCache.Set(key, value, DateTimeOffset.UtcNow.Add(_cacheExpiration));
        }
    }
}