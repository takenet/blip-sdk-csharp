using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions
{
    public class BuilderMessageReceiver : IMessageReceiver
    {
        private readonly IFlowMessageManager _flowMessageManager;
        private readonly BuilderSettings _settings;

        public BuilderMessageReceiver(IFlowMessageManager flowMessageManager, BuilderSettings settings)
        {
            _flowMessageManager = flowMessageManager;
            _settings = settings;
        }

        public Task ReceiveAsync(Message envelope, CancellationToken cancellationToken)
        {
            return _flowMessageManager.EnqueueMessageAsync(_settings.Flow, envelope.From.ToIdentity(), envelope, cancellationToken);
        }
    }
}