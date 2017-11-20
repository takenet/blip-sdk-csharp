using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder;
using Take.Blip.Client;

namespace Builder.Console
{
    public class BuilderMessageReceiver : IMessageReceiver
    {
        private readonly IFlowManager _flowManager;
        private readonly BuilderSettings _settings;

        public BuilderMessageReceiver(IFlowManager flowManager, BuilderSettings settings)
        {
            _flowManager = flowManager;
            _settings = settings;
        }

        public virtual Task ReceiveAsync(Message envelope, CancellationToken cancellationToken) 
            => _flowManager.ProcessInputAsync(envelope.Content, envelope.From.ToIdentity(), _settings.Flow, cancellationToken);
    }
}