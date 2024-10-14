using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.SendRawMessage
{
    public class SendRawMessageAction : ActionBase<SendRawMessageSettings>
    {
        private readonly ISender _sender;
        private readonly IDocumentSerializer _documentSerializer;

        public SendRawMessageAction(ISender sender, IDocumentSerializer documentSerializer)
            : base(nameof(SendRawMessage))
        {
            _sender = sender;
            _documentSerializer = documentSerializer;
        }

        public override Task ExecuteAsync(IContext context, SendRawMessageSettings settings, CancellationToken cancellationToken)
        {
            var message = new Message(null)
            {
                To = context.Input.Message.From,
                Content = _documentSerializer.Deserialize(settings.RawContent, settings.MediaType),
                Metadata = settings.Metadata
            };

            if (message.Content?.GetMediaType() != ChatState.MediaType)
            {
                message.Id = EnvelopeId.NewId();
            }

            if (context.Input.Message.From.Domain.Equals("tunnel.msging.net"))
            {
                message.Metadata ??= new Dictionary<string, string>();

                if (context.Input.Message.Metadata.TryGetValue("#tunnel.owner", out string owner))
                {
                    message.Metadata.Add("#tunnel.owner", owner);
                }

                if (context.Input.Message.Metadata.TryGetValue("#tunnel.originator", out string originator))
                {
                    message.Metadata.Add("#tunnel.originator", originator);
                }
            }

            return _sender.SendMessageAsync(message, cancellationToken);
        }
    }
}