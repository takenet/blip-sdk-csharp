using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Newtonsoft.Json.Linq;
using Take.Blip.Client.Extensions.Contacts;

namespace Take.Blip.Builder.Actions.SetContactProperty
{
    public class SetContactPropertyAction : IAction
    {
        private readonly IContactExtension _contactExtension;

        public SetContactPropertyAction(IContactExtension contactExtension)
        {
            _contactExtension = contactExtension;
        }

        public string Type => nameof(SetContactProperty);

        public Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var contact = settings.ToObject<Contact>(LimeSerializerContainer.Serializer);
            contact.Identity = contact.Identity;
            return _contactExtension.MergeAsync(context.User, contact, cancellationToken);
        }
    }
}
