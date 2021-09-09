using Lime.Messaging.Contents;
using Lime.Protocol;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Client;

namespace Take.Blip.Builder
{
    class RedirectManager : IRedirectManager
    {
        private readonly ISender _sender;

        public RedirectManager(ISender sender)
        {
            _sender = sender;
        }

        public Task RedirectUserAsync(IContext context, Redirect redirect, Identity contact, CancellationToken cancellationToken)
        {
            var metadata = new Dictionary<string, string>()
            {
                {  "contact", contact }
            };
            return _sender.SendMessageAsync(redirect, context.Input.Message.From, metadata, cancellationToken);
        }
    }
}
