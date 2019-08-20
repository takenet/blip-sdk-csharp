using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Take.Blip.Builder;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.Contacts;
using Take.Blip.Client.Extensions.Directory;
using Take.Blip.Client.Extensions.HelpDesk;
using Take.Blip.Client.Receivers;
using Takenet.Iris.Messaging.Resources;

namespace Builder.Console
{
    public class BuilderMessageReceiver : ContactMessageReceiverBase
    {
        private readonly IFlowManager _flowManager;
        private readonly BuilderSettings _settings;
        private readonly ISender _sender;
        private readonly IStateManager _stateManager;
        private readonly IDocumentSerializer _documentSerializer;
        private readonly IEnvelopeSerializer _envelopeSerializer;
        private readonly IContextProvider _contextProvider;
        private readonly IUserOwnerResolver _userOwnerResolver;
        private readonly IArtificialIntelligenceExtension _artificialIntelligenceExtension;

        public BuilderMessageReceiver(
            IFlowManager flowManager,
            BuilderSettings settings,
            IContactExtension contactExtension,
            IDirectoryExtension directoryExtension,
            ISender sender,
            IStateManager stateManager,
            IDocumentSerializer documentSerializer,
            IEnvelopeSerializer envelopeSerializer,
            IArtificialIntelligenceExtension artificialIntelligenceExtension,
            IContextProvider contextProvider, 
            IUserOwnerResolver userOwnerResolver)
            : base(contactExtension, directoryExtension)
        {
            _flowManager = flowManager;
            _settings = settings;
            _sender = sender;
            _stateManager = stateManager;
            _documentSerializer = documentSerializer;
            _envelopeSerializer = envelopeSerializer;
            _contextProvider = contextProvider;
            _userOwnerResolver = userOwnerResolver;
            _artificialIntelligenceExtension = artificialIntelligenceExtension;
        }

        protected override async Task ReceiveAsync(Message message, Contact contact, CancellationToken cancellationToken = new CancellationToken())
        {
            var inputMessage = message;

            // Check if is a message from a desk agent to the customer
            if (message.From.Name != null &&
                message.From.Domain != null &&
                message.From.Domain.Equals(HelpDeskExtension.DEFAULT_DESK_DOMAIN, StringComparison.OrdinalIgnoreCase))
            {
                // Check for ticket transfer
                if (message.Content is Ticket ticket && 
                    ticket.Status == TicketStatusEnum.Transferred)
                {
                    return;
                }                
                
                var originator = Node.Parse(Uri.UnescapeDataString(message.From.Name));

                // If the content is a ticket or redirect, change the message originator
                if (message.Content is Ticket || 
                    message.Content is Redirect)
                {
                    inputMessage = inputMessage.ShallowCopy();
                    inputMessage.From = originator;
                }
                else
                {
                    // If not, just forward the message directly to the originator, which is the ticket customer
                    await _sender.SendMessageAsync(
                        new Message()
                        {
                            Id = GetForwardId(message.Id),
                            To = originator,
                            Content = message.Content,
                            Metadata = message.Metadata
                        },
                        cancellationToken);

                    return;
                }
            }
            
            // Check for redirects (from desk or a tunnel)
            if (inputMessage.Content is Redirect redirect)
            {
                if (redirect.Context?.Value == null)
                {
                    return;
                }

                inputMessage = inputMessage.ShallowCopy();
                inputMessage.Content = redirect.Context.Value;
            }

            // Ignore chatstate composing and paused messages when not on desk
            if (message.Content is ChatState chatState && 
                (chatState.State == ChatStateEvent.Composing || chatState.State == ChatStateEvent.Paused))
            {
                // Determine if the current flow state is a desk state
                var state = await GetCurrentStateAsync(inputMessage, cancellationToken);
                if (state == null || 
                    !state.StartsWith("desk:", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            await _flowManager.ProcessInputAsync(inputMessage, _settings.Flow, cancellationToken);
        }

        private async Task<string> GetCurrentStateAsync(Message message, CancellationToken cancellationToken)
        {
            var (userIdentity, ownerIdentity) = await _userOwnerResolver.GetUserOwnerIdentitiesAsync(
                message, _settings.Flow.BuilderConfiguration, cancellationToken);

            var lazyInput = new LazyInput(
                message,
                userIdentity,
                _settings.Flow.BuilderConfiguration,
                _documentSerializer,
                _envelopeSerializer,
                _artificialIntelligenceExtension,
                cancellationToken);

            var context = _contextProvider.CreateContext(userIdentity, ownerIdentity, lazyInput, _settings.Flow);
            return await _stateManager.GetStateIdAsync(context, cancellationToken);
        }

        private static string GetForwardId(string messageId)
        {
            if (messageId == null) return null;
            return $"fwd:{messageId}";
        }
    }
}