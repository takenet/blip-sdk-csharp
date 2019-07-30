using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Messaging.Resources;
using System;
using System.Reflection;
using Lime.Protocol.Network;
using Take.Blip.Client.Extensions.Contacts;
using Take.Blip.Client.Extensions.Directory;
using Microsoft.Extensions.Caching.Memory;

namespace Take.Blip.Client.Receivers
{
    /// <summary>
    /// Defines a <see cref="IMessageReceiver"/> that includes contact information about the message sender.
    /// </summary>
    public abstract class ContactMessageReceiverBase : IMessageReceiver, IDisposable
    {
        private readonly IContactExtension _contactExtension;
        private readonly IDirectoryExtension _directoryExtension;
        private readonly bool _cacheLocally;
        private readonly TimeSpan _cacheExpiration;

        /// <summary>
        /// Initializes a new instance of <see cref="ContactMessageReceiverBase"/> class.
        /// </summary>
        /// <param name="contactExtension"></param>
        /// <param name="directoryExtension"></param>
        /// <param name="cacheLocally">Indicates if the contact information should be cached locally.</param>
        /// <param name="cacheExpiration">Defines the local cache expiration, if configured.</param>
        protected ContactMessageReceiverBase(
            IContactExtension contactExtension,
            IDirectoryExtension directoryExtension,
            bool cacheLocally = true,
            TimeSpan cacheExpiration = default)
        {
            _directoryExtension = directoryExtension;
            _contactExtension = contactExtension;
            _cacheLocally = cacheLocally;
            _cacheExpiration = cacheExpiration == default ? TimeSpan.FromMinutes(30) : cacheExpiration;
            ContactCache = new MemoryCache(new MemoryCacheOptions());
        }

        public virtual async Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = default(CancellationToken))
        {
            var identity = envelope.From.ToIdentity();
            var contact = await GetContactAsync(identity, cancellationToken);
            await ReceiveAsync(envelope, contact, cancellationToken);
        }
        
        protected MemoryCache ContactCache { get; }

        /// <summary>
        /// Gets the contact for the specified identity.
        /// </summary>
        protected virtual async Task<Contact> GetContactAsync(Identity identity, CancellationToken cancellationToken)
        {
            // First, tries get it from the cache, if configured.
            Contact contact = null;
            if (_cacheLocally)
            {
                contact = ContactCache.Get(identity.ToString()) as Contact;
            }

            if (contact == null)
            {
                try
                {
                    // Second, try from the roster.
                    contact = await _contactExtension.GetFromContactsOrDirectoryAsync(_directoryExtension, identity, cancellationToken);
                }
                catch (LimeException ex) when (
                    ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND ||
                    ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_SUPPORTED)
                {
                    contact = null;
                }
            }
            
            // Stores in the cache, if configured.
            if (contact != null && _cacheLocally)
            {
                ContactCache.Set(identity.ToString(), contact, DateTimeOffset.UtcNow.Add(_cacheExpiration));
            }

            return contact;
        }

        /// <summary>
        /// Receives a message with the contact information.
        /// </summary>
        protected abstract Task ReceiveAsync(Message message, Contact contact, CancellationToken cancellationToken = default(CancellationToken));
        
        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ContactCache.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
