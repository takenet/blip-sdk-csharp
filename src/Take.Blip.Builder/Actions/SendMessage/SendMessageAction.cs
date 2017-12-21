using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.SendMessage
{
    public class SendMessageAction : IAction
    {
        private readonly ISender _sender;

        public SendMessageAction(ISender sender)
        {
            _sender = sender;
        }

        public string Type => nameof(SendMessage);

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings), $"The settings are required for '{nameof(SendMessageAction)}' action");

            var message = new Message(EnvelopeId.NewId())
            {
                Id = EnvelopeId.NewId(),
                To = context.User.ToNode()
            };

            var mediaType = MediaType.Parse((string)settings[Message.TYPE_KEY]);
            var rawContent = settings[Message.CONTENT_KEY];

            if (mediaType.IsJson)
            {
                message.Content = new JsonDocument(rawContent.ToObject<Dictionary<string, object>>(), mediaType);
            }
            else
            {
                message.Content = new PlainDocument(rawContent.ToString(), mediaType);
            }

            if (settings.TryGetValue(Envelope.METADATA_KEY, out var metadata))
            {
                message.Metadata = ((JObject) metadata).ToObject<Dictionary<string, string>>();
            }
            
            await _sender.SendMessageAsync(message, cancellationToken);

            // Await the interval if it is a chatstate message
            if (mediaType == ChatState.MediaType)
            {
                var chatState = rawContent.ToObject<ChatState>(LimeSerializerContainer.Serializer);
                if (chatState.Interval != null)
                {
                    await Task.Delay(chatState.Interval.Value, cancellationToken);
                }
            }
        }
    }
}
