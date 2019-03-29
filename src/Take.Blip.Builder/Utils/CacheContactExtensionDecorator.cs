using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
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
        private readonly ICacheOwnerCallerContactMap _cacheContactMap;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly Application _applicationSettings;

        public CacheContactExtensionDecorator(
            IContactExtension contactExtension,
            ICacheOwnerCallerContactMap cacheContactMap,
            ILogger _logger,
            IConfiguration configuration,
            Application applicationSettings)
        {
            _contactExtension = contactExtension;
            _cacheContactMap = cacheContactMap;
            _configuration = configuration;
            _applicationSettings = applicationSettings;
        }

        public async Task<Contact> GetAsync(Identity identity, CancellationToken cancellationToken)
        {
            var key = CreateKey(_applicationSettings, identity);
            var contact = await _cacheContactMap.GetValueOrDefaultAsync(key, cancellationToken);

            if (contact == null)
            {
                contact = await _contactExtension.GetAsync(identity, cancellationToken);
                try
                {
                    await _cacheContactMap.TryAddAsync(key, contact, true, cancellationToken);
                    await _cacheContactMap.SetRelativeKeyExpirationAsync(key, _configuration.ContactCacheExpiration);
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error adding contact {identity} for owner {GetApplicationIdentity(_applicationSettings)} on CacheOwnerCallerContactMap");
                }
            }

            return contact;
        }

        public async Task MergeAsync(Identity identity, Contact contact, CancellationToken cancellationToken)
        {
            await _contactExtension.MergeAsync(identity, contact, cancellationToken);
            await RemoveFromCache(identity, cancellationToken);
        }

        public async Task SetAsync(Identity identity, Contact contact, CancellationToken cancellationToken)
        {
            await _contactExtension.SetAsync(identity, contact, cancellationToken);

            var key = CreateKey(_applicationSettings, identity);
            await _cacheContactMap.TryAddAsync(key, contact, true, cancellationToken);
            await _cacheContactMap.SetRelativeKeyExpirationAsync(key, _configuration.ContactCacheExpiration);
        }

        public async Task DeleteAsync(Identity identity, CancellationToken cancellationToken)
        {
            await _contactExtension.DeleteAsync(identity, cancellationToken);
            await RemoveFromCache(identity, cancellationToken);
        }

        private async Task RemoveFromCache(Identity identity, CancellationToken cancellationToken)
        {
            var key = CreateKey(_applicationSettings, identity);
            await _cacheContactMap.TryRemoveAsync(key, cancellationToken);
        }

        private OwnerCaller CreateKey(Application applicationSettings, Identity identity) =>
            OwnerCaller.Create(GetApplicationIdentity(applicationSettings), identity);

        private Identity GetApplicationIdentity(Application applicationSettings) =>
            new Identity(applicationSettings.Identifier, applicationSettings.Domain ?? Constants.DEFAULT_DOMAIN);
    }
}