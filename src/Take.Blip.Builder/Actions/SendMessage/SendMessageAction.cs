using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.SendMessage
{
    public class SendMessageAction : IAction
    {
        private readonly ISender _sender;
        private readonly IBlipLogger _blipMonitoringLogger;

        public SendMessageAction(ISender sender, IBlipLogger? blipMonitoringLogger = null)
        {
            _sender = sender;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public string Type => nameof(SendMessage);

        public string[]? OutputVariables => null;

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings), $"The settings are required for '{nameof(SendMessageAction)}' action");

            try
            {
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
                    message.Metadata = ((JObject)metadata).ToObject<Dictionary<string, string>>();
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

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "SendMessage",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["contentType"] = (string)settings[Message.TYPE_KEY],
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
                    Title = "SendMessage",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["contentType"] = (string)settings[Message.TYPE_KEY],
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
