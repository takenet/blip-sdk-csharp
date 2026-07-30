using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.SendRawMessage
{
    public class SendRawMessageAction : ActionBase<SendRawMessageSettings>
    {
        private readonly ISender _sender;
        private readonly IDocumentSerializer _documentSerializer;
        private readonly IBlipLogger _blipMonitoringLogger;

        public SendRawMessageAction(ISender sender, IDocumentSerializer documentSerializer, IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(SendRawMessage))
        {
            _sender = sender;
            _documentSerializer = documentSerializer;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(IContext context, SendRawMessageSettings settings, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            try
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

                await _sender.SendMessageAsync(message, cancellationToken);

                this.LogDelivery(_blipMonitoringLogger, context, new JObject
                {
                    ["mediaType"] = settings.MediaType?.ToString(),
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                });
            }
            catch (Exception ex)
            {
                this.LogError(_blipMonitoringLogger, context, new JObject
                {
                    ["mediaType"] = settings.MediaType?.ToString(),
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }, ex);
                throw;
            }
        }
    }
}
