using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Network;
using Take.Blip.Client.Extensions.Contacts;

namespace Take.Blip.Builder.Variables
{
    public class ContactVariableProvider : IVariableProvider
    {
        public const string CONTACT_EXTRAS_VARIABLE_PREFIX = "extras.";
        
        private readonly ConcurrentDictionary<string, PropertyInfo> _contactPropertyCacheDictionary;
        private readonly IContactExtension _contactExtension;

        public ContactVariableProvider(IContactExtension contactExtension)
        {
            _contactExtension = contactExtension;
            _contactPropertyCacheDictionary = new ConcurrentDictionary<string, PropertyInfo>();
        }

        public VariableSource Source => VariableSource.Contact;

        public async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
            Contact contact;
            try
            {
                if (context.InputContext.TryGetValue(nameof(contact), out var contactValue))
                {
                    contact = (Contact)contactValue;
                }
                else
                {
                    contact = await _contactExtension.GetAsync(context.User, cancellationToken);
                    context.InputContext[nameof(contact)] = contact;
                }

                if (contact == null) return null;
                
                return GetContactProperty(contact, name);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                context.InputContext[nameof(contact)] = null;
                return null;
            }
        }

        private string GetContactProperty(Contact contact, string variableName)
        {
            if (variableName.StartsWith(CONTACT_EXTRAS_VARIABLE_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                var extraVariableName = variableName.Remove(0, CONTACT_EXTRAS_VARIABLE_PREFIX.Length);
                if (contact.Extras != null && contact.Extras.TryGetValue(extraVariableName, out var extraVariableValue))
                {
                    return extraVariableValue;
                }
                return null;
            }

            var contactPropertyInfo = GetContactPropertyInfo(variableName.ToLowerInvariant());
            if (contactPropertyInfo != null) return contactPropertyInfo.GetValue(contact)?.ToString();

            return null;
        }

        private PropertyInfo GetContactPropertyInfo(string contactVariableName)
        {
            // Caches the properties to reduce the reflection overhead
            if (!_contactPropertyCacheDictionary.TryGetValue(contactVariableName, out var property))
            {
                property = typeof(Contact).GetProperty(contactVariableName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property != null) _contactPropertyCacheDictionary.TryAdd(contactVariableName, property);
            }

            return property;
        }
    }
}