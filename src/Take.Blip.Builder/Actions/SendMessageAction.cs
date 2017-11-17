using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions
{
    public class SendMessageAction : IAction
    {
        private readonly ISender _sender;

        public SendMessageAction(ISender sender)
        {
            _sender = sender;
        }

        public string Type => "SendMessage";

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            var message = new Message(EnvelopeId.NewId())
            {
                Id = EnvelopeId.NewId(),
                To = context.User.ToNode()
            };

            var mediaType = MediaType.Parse((string)settings[Message.TYPE_KEY]);
            var contentJToken = settings[Message.CONTENT_KEY];

            if (mediaType.IsJson)
            {
                message.Content = new JsonDocument(contentJToken.ToObject<IDictionary<string, object>>(), mediaType);
            }
            else
            {
                message.Content = new PlainDocument(contentJToken.ToObject<string>(), mediaType);
            }
            
            await _sender.SendMessageAsync(message, cancellationToken);
        }
    }
}
