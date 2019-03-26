using System;
using Lime.Messaging.Resources;
using Microsoft.Extensions.Caching.Memory;

namespace Take.Blip.Builder.Utils
{
    public class ContactMemoryCache : ICache<Contact>
    {
        private readonly MemoryCache _contactCache;

        public ContactMemoryCache()
        {
            _contactCache = new MemoryCache(new MemoryCacheOptions());
        }

        public Contact Get(object key) => _contactCache.Get(key) as Contact;

        public void Remove(object key)
        {
            throw new NotImplementedException();
        }

        public void Set(object key, Contact value, TimeSpan cacheExpiration)
        {
            _contactCache.Set(key, value, DateTimeOffset.UtcNow.Add(cacheExpiration));
        }
    }
}