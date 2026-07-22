using System;
using System.Collections.Generic;
using System.Threading;
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

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "SendRawMessage",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["mediaType"] = settings.MediaType?.ToString(),
                        ["success"] = true,
                    },
                    FlowVersion = context.Flow?.Version,
                    Channel = context.Input.Message?.From?.Domain,
                    IdMessage = context.Input.Message?.Id,
                    From = context.UserIdentity?.ToString(),
                    To = context.OwnerIdentity?.ToString(),
                    OriginalFrom = context.Input.Message?.From,
                    OriginalTo = context.Input.Message?.To,
                });
            }
            catch (Exception ex)
            {
                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "SendRawMessage",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["mediaType"] = settings.MediaType?.ToString(),
                        ["success"] = false,
                        ["error"] = ex.ToString(),
                    },
                    FlowVersion = context.Flow?.Version,
                    Channel = context.Input.Message?.From?.Domain,
                    IdMessage = context.Input.Message?.Id,
                    From = context.UserIdentity?.ToString(),
                    To = context.OwnerIdentity?.ToString(),
                    OriginalFrom = context.Input.Message?.From,
                    OriginalTo = context.Input.Message?.To,
                });
                throw;
            }
        }
    }
}
