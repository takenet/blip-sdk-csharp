using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder;
using Take.Blip.Client;
using Take.Blip.Client.Activation;

namespace Builder.Console
{
    public class BuilderMessageReceiver : IMessageReceiver
    {
        private readonly IFlowManager _flowManager;
        private readonly BuilderSettings _settings;
        private readonly Identity _applicationIdentity;

        public BuilderMessageReceiver(IFlowManager flowManager, BuilderSettings settings, Application application)
        {
            _flowManager = flowManager;
            _settings = settings;
            _applicationIdentity = $"{application.Identifier}@{application.Domain ?? Constants.DEFAULT_DOMAIN}";
        }

        public virtual Task ReceiveAsync(Message envelope, CancellationToken cancellationToken) 
            => _flowManager.ProcessInputAsync(envelope.Content, envelope.From.ToIdentity(), _applicationIdentity, _settings.Flow, cancellationToken);
    }
}