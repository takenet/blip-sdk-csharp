using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.EventTracker;

namespace Take.Blip.Builder.Actions.TrackEvent
{
    public class TrackEventAction : ActionBase<TrackEventSettings>
    {
        private readonly IEventTrackExtension _eventTrackExtension;
        private readonly IBlipLogger _blipMonitoringLogger;

        public TrackEventAction(IEventTrackExtension eventTrackExtension, IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(TrackEvent))
        {
            _eventTrackExtension = eventTrackExtension;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(IContext context, TrackEventSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                await _eventTrackExtension.AddAsync(
                    settings.Category,
                    settings.Action,
                    settings.Label,
                    value: settings.ParsedValue,
                    messageId: EnvelopeReceiverContext<Message>.Envelope?.Id,
                    extras: settings.Extras,
                    contactIdentity: context.UserIdentity,
                    fireAndForget: settings.FireAndForget ?? true,
                    cancellationToken: cancellationToken
                    );

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "TrackEvent",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["category"] = settings.Category,
                        ["action"] = settings.Action,
                        ["label"] = settings.Label,
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
                    Title = "TrackEvent",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["category"] = settings.Category,
                        ["action"] = settings.Action,
                        ["label"] = settings.Label,
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
