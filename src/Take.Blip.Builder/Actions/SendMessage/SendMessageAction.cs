using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Newtonsoft.Json;
using Take.Blip.Client;
using JsonDocument = Lime.Protocol.JsonDocument;

namespace Take.Blip.Builder.Actions.SendMessage
{
    public class SendMessageAction : ActionBase<JsonElement>
    {
        private readonly ISender _sender;

        public SendMessageAction(ISender sender) : base(nameof(SendMessage))
        {
            _sender = sender;
        }

        public override async Task ExecuteAsync(IContext context, JsonElement settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var message = new Message(null)
            {
                To = context.Input.Message.From
            };

            var mediaType = MediaType.Parse(settings.GetProperty(Message.TYPE_KEY).GetString());
            var content = settings.GetProperty(Message.CONTENT_KEY);

            if (mediaType.IsJson)
            {
                message.Content = new JsonDocument(content.ToObject<Dictionary<string, object>>(), mediaType);
            }
            else
            {
                message.Content = new PlainDocument(content.GetString(), mediaType);
            }

            if (settings.TryGetProperty(Envelope.METADATA_KEY, out var metadata))
            {
                message.Metadata = metadata.ToObject<Dictionary<string, string>>();
            }

            var isChatState = mediaType == ChatState.MediaType;
            if (!isChatState)
            {
                message.Id = EnvelopeId.NewId();
            }

            await _sender.SendMessageAsync(message, cancellationToken);

            // Await the interval if it is a chatstate message
            if (isChatState)
            {
                var chatState = JsonConvert.DeserializeObject<ChatState>(content.GetRawText());
                if (chatState.Interval != null)
                {
                    await Task.Delay(chatState.Interval.Value, cancellationToken);
                }
            }
        }
    }
}
