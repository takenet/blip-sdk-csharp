using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using SmartFormat;

namespace Take.Blip.Client.Extensions.Contacts
{
    public class ContactExtension : ExtensionBase, IContactExtension
    {
        private readonly ISender _messagingHubSender;

        public ContactExtension(ISender messagingHubSender)
            : base(messagingHubSender)
        {
            _messagingHubSender = messagingHubSender;
        }

        public Task<Contact> GetAsync(Identity identity, CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            var requestCommand = CreateGetCommandRequest(
                Smart.Format(UriTemplates.CONTACT, new { contactIdentity = Uri.EscapeDataString(identity.ToString()) }));

            return ProcessCommandAsync<Contact>(requestCommand, cancellationToken);
        }

        public Task SetAsync(Identity identity, Contact contact, CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (contact == null) throw new ArgumentNullException(nameof(contact));

            var requestCommand = CreateSetCommandRequest(
                contact,
                UriTemplates.CONTACTS);

            return ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public Task DeleteAsync(Identity identity, CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            var requestCommand = CreateDeleteCommandRequest(
                Smart.Format(UriTemplates.CONTACT, new { contactIdentity = Uri.EscapeDataString(identity.ToString()) }));

            return ProcessCommandAsync(requestCommand, cancellationToken);
        }
    }
}
