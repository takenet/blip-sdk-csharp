using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions.Redirect
{
    public class RedirectAction : IAction
    {
        private readonly IRedirectManager _redirectManager;

        public RedirectAction(IRedirectManager redirectManager)
        {
            _redirectManager = redirectManager;
        }

        public string Type => nameof(Redirect);

        public Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var redirect = settings.ToObject<Lime.Messaging.Contents.Redirect>(LimeSerializerContainer.Serializer);
            return _redirectManager.RedirectUserAsync(context, redirect, cancellationToken);
        }
    }
}
