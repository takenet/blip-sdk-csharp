using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Newtonsoft.Json.Linq;
using Take.Blip.Client.Extensions.Contacts;

namespace Take.Blip.Builder.Actions.MergeContact
{
    public class MergeContactAction : IAction
    {
        private readonly IContactExtension _contactExtension;

        public MergeContactAction(IContactExtension contactExtension)
        {
            _contactExtension = contactExtension;
        }

        public string Type => nameof(MergeContact);

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var contact = settings.ToObject<Contact>(LimeSerializerContainer.Serializer);
            contact.Identity = contact.Identity;
            await _contactExtension.MergeAsync(context.User, contact, cancellationToken);
            context.InputContext.Remove(nameof(contact));
        }
    }
}