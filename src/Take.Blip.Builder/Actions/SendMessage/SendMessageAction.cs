﻿using System;
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

            var message = new Message(null)
            {
                To = context.Input.Message.From
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

            var isChatState = mediaType == ChatState.MediaType;
            if (!isChatState)
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

            await _sender.SendMessageAsync(message, cancellationToken);

            // Await the interval if it is a chatstate message
            if (isChatState)
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
