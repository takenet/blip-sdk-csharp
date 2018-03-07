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
        public const string ID_PREFIX = "fwd";

        private readonly ISender _sender;

        public ForwardMessageToDeskAction(ISender sender) 
            : base(nameof(ForwardMessageToDesk))
        {
            _sender = sender;
        }

        public override Task ExecuteAsync(IContext context, ForwardMessageToDeskSettings settings, CancellationToken cancellationToken)
        {
            var message = new Message
            {
                Id = $"{ID_PREFIX}:{EnvelopeReceiverContext<Message>.Envelope?.Id ?? EnvelopeId.NewId()}",
                To = new Node(Uri.EscapeDataString(context.User), settings.DeskDomain ?? DEFAULT_DESK_DOMAIN, null),
                Content = context.Input.Content
            };
            
            return _sender.SendMessageAsync(message, cancellationToken);
        }
    }
}
