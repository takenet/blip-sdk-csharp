using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.ForwardMessageToDesk
{
    public class ForwardMessageToDeskAction : ActionBase<ForwardMessageToDeskSettings>
    {
        public const string DEFAULT_DESK_DOMAIN = "desk." + Constants.DEFAULT_DOMAIN;

        private readonly ISender _sender;
        private readonly IStateManager _stateManager;
        
        public ForwardMessageToDeskAction(ISender sender, IStateManager stateManager) 
            : base(nameof(ForwardMessageToDesk))
        {
            _sender = sender;
            _stateManager = stateManager;
        }

        public override async Task ExecuteAsync(IContext context, ForwardMessageToDeskSettings settings, CancellationToken cancellationToken)
        {
            // Detects if the the previous state is different than the current. If it is the case, we should ignore
            // the user input and forward the context to the attendant. 
            if (await _stateManager.GetPreviousStateIdAsync(context.Flow.Id, context.User, cancellationToken) !=
                await _stateManager.GetStateIdAsync(context.Flow.Id, context.User, cancellationToken))
            {

            }


            var message = new Message
            {
                Id = EnvelopeReceiverContext<Message>.Envelope?.Id ?? EnvelopeId.NewId(),
                To = new Node(Uri.EscapeDataString(context.User), settings.Domain ?? DEFAULT_DESK_DOMAIN, null),
                Content = context.Input.Content
            };
            
            await _sender.SendMessageAsync(message, cancellationToken);
        }
    }
}
