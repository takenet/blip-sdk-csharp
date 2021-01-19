using Lime.Messaging.Contents;
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

        public Task RedirectUserAsync(IContext context, Redirect redirect, CancellationToken cancellationToken)
        {
            return _sender.SendMessageAsync(redirect, context.Input.Message.From, cancellationToken);
        }
    }
}
