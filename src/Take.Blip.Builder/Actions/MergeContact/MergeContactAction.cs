using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Newtonsoft.Json.Linq;
using Serilog;
using Take.Blip.Client.Extensions.Contacts;

namespace Take.Blip.Builder.Actions.MergeContact
{
    public class MergeContactAction : IAction
    {
        private readonly IContactExtension _contactExtension;
        private readonly ILogger _logger;

        public MergeContactAction(IContactExtension contactExtension, ILogger logger)
        {
            _contactExtension = contactExtension;
            _logger = logger;
        }

        public string Type => nameof(MergeContact);

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var contact = settings.ToObject<Contact>(LimeSerializerContainer.Serializer);
            contact.Identity = contact.Identity;

            var informationMessage = $"Trying to merge contact values ({settings}) for UserIdentity {context.UserIdentity}";
            _logger.Information(informationMessage);

            await _contactExtension.MergeAsync(context.UserIdentity, contact, cancellationToken);
            context.RemoveContact();
        }
    }
}