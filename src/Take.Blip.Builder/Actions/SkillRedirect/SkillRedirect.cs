using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Take.Blip.Client;
using Take.Blip.Client.Activation;

namespace Take.Blip.Builder.Actions.SkillRedirect
{
    public class SkillRedirectAction : IAction
    {
        private readonly ISender _sender;
        private readonly Application _application;

        public SkillRedirectAction(ISender sender, Application application)
        {
            _sender = sender;
            _application = application;
        }

        public string Type => nameof(SkillRedirect);

        public Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var redirect = settings.ToObject<Lime.Messaging.Contents.Redirect>(LimeSerializerContainer.Serializer);
            return _sender.SendMessageAsync(redirect, _application.Node, cancellationToken);
        }
    }
}
