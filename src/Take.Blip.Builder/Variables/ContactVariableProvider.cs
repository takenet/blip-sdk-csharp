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
    public class ContactVariableProvider : UserVariableProviderBase<Contact>
    {
        public const string CONTACT_EXTRAS_VARIABLE_PREFIX = "extras.";
        
        private readonly IContactExtension _contactExtension;

        public ContactVariableProvider(IContactExtension contactExtension)
            : base(VariableSource.Contact)
        {
            _contactExtension = contactExtension;
        }

        protected override Task<Contact> GetAsync(Identity userIdentity, CancellationToken cancellationToken) 
            => _contactExtension.GetAsync(userIdentity, cancellationToken);

        protected override string GetProperty(Contact item, string propertyName)
        {
            if (propertyName.StartsWith(CONTACT_EXTRAS_VARIABLE_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                var extraVariableName = propertyName.Remove(0, CONTACT_EXTRAS_VARIABLE_PREFIX.Length);
                if (item.Extras != null && item.Extras.TryGetValue(extraVariableName, out var extraVariableValue))
                {
                    return extraVariableValue;
                }
                return null;
            }
            
            return base.GetProperty(item, propertyName);
        }
    }
}