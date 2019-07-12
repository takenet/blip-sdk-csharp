using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Take.Blip.Builder;
using Take.Blip.Client;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.Contacts;
using Take.Blip.Client.Extensions.Directory;
using Take.Blip.Client.Receivers;

namespace Builder.Console
{
    public class BuilderMessageReceiver : ContactMessageReceiverBase
    {
        private readonly IFlowManager _flowManager;
        private readonly BuilderSettings _settings;
        private readonly Identity _applicationIdentity;

        public BuilderMessageReceiver(
            IContactExtension contactExtension,
            IDirectoryExtension directoryExtension,
            IFlowManager flowManager,
            BuilderSettings settings,
            Application application)
            : base(contactExtension, directoryExtension)
        {
            _flowManager = flowManager;
            _settings = settings;
            _applicationIdentity = $"{application.Identifier}@{application.Domain ?? Constants.DEFAULT_DOMAIN}";
        }

        protected override Task ReceiveAsync(Message message, Contact contact, CancellationToken cancellationToken)
            => _flowManager.ProcessInputAsync(message, _settings.Flow, contact, cancellationToken);

    }
}