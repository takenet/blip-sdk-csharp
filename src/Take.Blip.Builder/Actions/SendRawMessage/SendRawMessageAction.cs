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
                To = context.User.ToNode(),
                Content = _documentSerializer.Deserialize(settings.RawContent, settings.MediaType),
                Metadata = settings.Metadata
            };

            if (message.Content?.GetMediaType() != ChatState.MediaType)
            {
                message.Id = EnvelopeId.NewId();
            }

            return _sender.SendMessageAsync(message, cancellationToken);
        }
    }
}