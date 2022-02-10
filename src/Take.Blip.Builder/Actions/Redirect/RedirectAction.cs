using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Take.Blip.Builder.Actions.Redirect
{
    public class RedirectAction : IAction
    {
        private readonly IRedirectManager _redirectManager;
        private readonly ILogger _logger;

        public RedirectAction(IRedirectManager redirectManager, ILogger logger)
        {
            _redirectManager = redirectManager;
            _logger = logger;
        }

        public string Type => nameof(Redirect);

        public Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var redirect = settings.ToObject<Lime.Messaging.Contents.Redirect>(LimeSerializerContainer.Serializer);
            if (context.Input.Message.Metadata != null && 
                context.Input.Message.Metadata.TryGetValue("REDIRECT_TEST_LOG", out var metadataRedirectAddress) && 
                redirect.Address == metadataRedirectAddress) 
            {
                _logger.Warning($"#REDIRECT_LOG# ({context.Input.Message}) - ({redirect.Address}) - ({redirect.Context})");
            }
            return _redirectManager.RedirectUserAsync(context, redirect, cancellationToken);
        }
    }
}
