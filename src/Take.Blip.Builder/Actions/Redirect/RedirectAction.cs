using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.Redirect
{
    public class RedirectAction : ActionBase<JsonElement>
    {
        private readonly ISender _sender;

        public RedirectAction(ISender sender) : base(nameof(Redirect))
        {
            _sender = sender;
        }

        public override Task ExecuteAsync(IContext context, JsonElement settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var redirect = GetRedirectFromJsonDocument(settings);
            return _sender.SendMessageAsync(redirect, context.Input.Message.From, cancellationToken);
        }

        private static Lime.Messaging.Contents.Redirect GetRedirectFromJsonDocument(JsonElement settings)
        {
            var rawText = settings.GetRawText();
            return JsonConvert.DeserializeObject<Lime.Messaging.Contents.Redirect>(rawText);
        }
    }
}
