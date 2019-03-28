using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Microsoft.Extensions.Caching.Memory;
using Take.Blip.Client.Extensions.Contacts;

namespace Take.Blip.Builder.Utils
{
    public class CacheContactExtensionDecorator : IContactExtension
    {
        private readonly IContactExtension _contactExtension;
        private readonly MemoryCache _contactCache;
        private TimeSpan _cacheExpiration;

        public CacheContactExtensionDecorator(IContactExtension contactExtension)
        {
            _contactExtension = contactExtension;
            _contactCache = new MemoryCache(new MemoryCacheOptions());
            _cacheExpiration = TimeSpan.FromMinutes(30);
        }

        public async Task<Contact> GetAsync(Identity identity, CancellationToken cancellationToken)
        {
            Contact contact = _contactCache.Get(identity.ToString()) as Contact;

            if (contact == null)
            {
                contact = await _contactExtension.GetAsync(identity, cancellationToken);
                _contactCache.Set(identity.ToString(), contact, DateTimeOffset.UtcNow.Add(_cacheExpiration));
            }

            return contact;
        }

        public async Task MergeAsync(Identity identity, Contact contact, CancellationToken cancellationToken)
        {
            await _contactExtension.MergeAsync(identity, contact, cancellationToken);
            _contactCache.Remove(identity.ToString());
        }

        public async Task SetAsync(Identity identity, Contact contact, CancellationToken cancellationToken)
        {
            await _contactExtension.SetAsync(identity, contact, cancellationToken);
            _contactCache.Set(identity.ToString(), contact, DateTimeOffset.UtcNow.Add(_cacheExpiration));
        }

        public async Task DeleteAsync(Identity identity, CancellationToken cancellationToken)
        {
            await _contactExtension.DeleteAsync(identity, cancellationToken);
            _contactCache.Remove(identity.ToString());
        }
    }
}