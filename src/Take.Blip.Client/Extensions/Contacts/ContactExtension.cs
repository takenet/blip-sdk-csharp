using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using SmartFormat;
using static Lime.Messaging.Resources.UriTemplates;

namespace Take.Blip.Client.Extensions.Contacts
{
    public class ContactExtension : ExtensionBase, IContactExtension
    {
        private static readonly Node CrmAddress = Node.Parse($"postmaster@crm.{Constants.DEFAULT_DOMAIN}");


        public ContactExtension(ISender sender)
            : base(sender)
        {
        }

        public Task<Contact> GetAsync(Identity identity, CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            var requestCommand = CreateGetCommandRequest(
                Smart.Format(CONTACT, new { contactIdentity = Uri.EscapeDataString(identity.ToString()) }),
                CrmAddress);

            return ProcessCommandAsync<Contact>(requestCommand, cancellationToken);
        }

        public Task SetAsync(Identity identity, Contact contact, CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (contact == null) throw new ArgumentNullException(nameof(contact));

            if (contact.Identity == null) contact.Identity = identity;

            var requestCommand = CreateSetCommandRequest(
                contact,
                CONTACTS,
                CrmAddress);

            return ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public Task MergeAsync(Identity identity, Contact contact, CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (contact == null) throw new ArgumentNullException(nameof(contact));

            if (contact.Identity == null) contact.Identity = identity;

            var requestCommand = CreateMergeCommandRequest(
                contact,
                CONTACTS,
                CrmAddress);

            return ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public Task DeleteAsync(Identity identity, CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            var requestCommand = CreateDeleteCommandRequest(
                Smart.Format(CONTACT, new { contactIdentity = Uri.EscapeDataString(identity.ToString()) }),
                CrmAddress);

            return ProcessCommandAsync(requestCommand, cancellationToken);
        }
    }
}
