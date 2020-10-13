using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Newtonsoft.Json;
using Take.Blip.Client.Extensions.Contacts;

namespace Take.Blip.Builder.Actions.MergeContact
{
    public class MergeContactAction : ActionBase<JsonElement>
    {
        private readonly IContactExtension _contactExtension;

        public MergeContactAction(IContactExtension contactExtension) : base(nameof(MergeContact))
        {
            _contactExtension = contactExtension;
        }

        public override async Task ExecuteAsync(IContext context, JsonElement settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var contact = GetContactFromJsonDocument(settings);
            await _contactExtension.MergeAsync(context.UserIdentity, contact, cancellationToken);
            context.RemoveContact();
        }

        private static Contact GetContactFromJsonDocument(JsonElement settings)
        {
            var rawText = settings.GetRawText();
            return JsonConvert.DeserializeObject<Contact>(rawText);
        }
    }
}