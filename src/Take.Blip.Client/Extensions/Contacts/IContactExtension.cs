using Lime.Messaging.Resources;
using Lime.Protocol;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Network;
using Take.Blip.Client.Extensions.Directory;

namespace Take.Blip.Client.Extensions.Contacts
{
    /// <summary>
    /// Manage contact's roster
    /// </summary>
    public interface IContactExtension
    {
        /// <summary>
        /// Get contact info from roster
        /// </summary>
        /// <param name="identity">Contact identity</param>
        /// <returns>Contact info</returns>
        Task<Contact> GetAsync(Identity identity, CancellationToken cancellationToken);

        /// <summary>
        /// Set contact info into the bot roster.
        /// </summary>
        /// <param name="identity">Contact identity</param>
        /// <param name="contact">Contact info</param>
        Task SetAsync(Identity identity, Contact contact, CancellationToken cancellationToken);

        /// <summary>
        /// Merge the contact info into the bot roster.
        /// </summary>
        /// <param name="identity">Contact identity</param>
        /// <param name="contact">Contact info</param>
        Task MergeAsync(Identity identity, Contact contact, CancellationToken cancellationToken);

        /// <summary>
        /// Delete a contact info from roster
        /// </summary>
        /// <param name="identity">Contact identity</param>
        Task DeleteAsync(Identity identity, CancellationToken cancellationToken);
    }

    public static class ContactExtensionExtensions
    {
        public static async Task<Contact> GetFromContactsOrDirectoryAsync(
            this IContactExtension contactExtension,
            IDirectoryExtension directoryExtension,
            Identity identity,
            CancellationToken cancellationToken)
        {
            if (contactExtension == null) throw new ArgumentNullException(nameof(contactExtension));
            if (directoryExtension == null) throw new ArgumentNullException(nameof(directoryExtension));
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            
            Contact contact = null;
            try
            {
                // try from the roster.
                contact = await contactExtension.GetAsync(identity, cancellationToken);
            }
            catch (LimeException ex) when (
                ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND || 
                ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_SUPPORTED) { }

            if (contact == null)
            {
                // Try from the directory.
                var account = await directoryExtension.GetDirectoryAccountAsync(identity, cancellationToken);
                if (account == null) // Should never occur because the extension should throw the exception itself
                {
                    throw new LimeException(ReasonCodes.COMMAND_RESOURCE_NOT_FOUND, "The account was not found in the directory");
                }
                
                contact = new Contact
                {
                    Identity = identity, Name = account.FullName
                };
                
                foreach (var property in typeof(ContactDocument).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var accountValue = property.GetValue(account);
                    property.SetValue(contact, accountValue);
                }
            }

            return contact;
        } 
    }
}
