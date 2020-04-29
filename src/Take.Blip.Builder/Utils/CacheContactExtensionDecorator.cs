using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Newtonsoft.Json;
using Serilog;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Storage;
using Take.Blip.Client;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.Contacts;

namespace Take.Blip.Builder.Utils
{
    public class CacheContactExtensionDecorator : IContactExtension
    {
        private readonly IContactExtension _contactExtension;
        private readonly IOwnerCallerContactMap _contactMap;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public CacheContactExtensionDecorator(
            IContactExtension contactExtension,
            IOwnerCallerContactMap contactMap,
            ILogger logger,
            IConfiguration configuration)
        {
            _contactExtension = contactExtension;
            _contactMap = contactMap;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Contact> GetAsync(Identity identity, CancellationToken cancellationToken)
        {
            if (!_configuration.ContactCacheEnabled)
            {
                return await _contactExtension.GetAsync(identity, cancellationToken);
            }
            
            var key = CreateKey(identity);
            
            Contact contact = null;

            try
            {
                 contact = await _contactMap.GetValueOrDefaultAsync(key, cancellationToken);
            }
            catch (JsonSerializationException e) {}

            if (contact == null)
            {
                contact =  await _contactExtension.GetAsync(identity, cancellationToken);

                if (contact != null)
                {
                    await AddToCacheAsync(contact, key, cancellationToken);
                }
            }
            
            return contact;
        }

        public async Task MergeAsync(Identity identity, Contact contact, CancellationToken cancellationToken)
        {
            await _contactExtension.MergeAsync(identity, contact, cancellationToken);
            await RemoveFromCacheAsync(identity, cancellationToken);
        }

        public async Task SetAsync(Identity identity, Contact contact, CancellationToken cancellationToken)
        {
            await _contactExtension.SetAsync(identity, contact, cancellationToken);

            var key = CreateKey(identity);

            if (contact != null)
            {
                await AddToCacheAsync(contact, key, cancellationToken);   
            }
        }

        public async Task DeleteAsync(Identity identity, CancellationToken cancellationToken)
        {
            await _contactExtension.DeleteAsync(identity, cancellationToken);
            await RemoveFromCacheAsync(identity, cancellationToken);
        }

        private async Task RemoveFromCacheAsync(Identity identity, CancellationToken cancellationToken)
        {
            var key = CreateKey(identity);
            await _contactMap.TryRemoveAsync(key, cancellationToken);
        }

        private async Task AddToCacheAsync(Contact contact, OwnerCaller key, CancellationToken cancellationToken)
        {
            try
            {
                await _contactMap.TryAddAsync(key, contact, true, cancellationToken);
                await _contactMap.SetRelativeKeyExpirationAsync(key, _configuration.ContactCacheExpiration);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex, 
                    "Error adding contact {{identity}} for owner {{application}} on CacheOwnerCallerContactMap",
                    contact.Identity,
                    OwnerContext.Owner);
            }
        }

        private static OwnerCaller CreateKey(Identity identity) => OwnerCaller.Create(OwnerContext.Owner, identity);
        
    }
}