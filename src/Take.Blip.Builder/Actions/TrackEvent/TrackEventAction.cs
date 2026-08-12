using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Blip.Ai.Bot.Monitoring.Logging.Abstractions;
using Blip.Ai.Bot.Monitoring.Logging.Abstractions.Models;
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
            var sw = Stopwatch.StartNew();
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

                this.LogExecution(_blipMonitoringLogger, context, new JObject
                {
                    ["category"] = settings.Category,
                    ["action"] = settings.Action,
                    ["label"] = settings.Label,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                });
            }
            catch (Exception ex)
            {
                this.LogError(_blipMonitoringLogger, context, new JObject
                {
                    ["category"] = settings.Category,
                    ["action"] = settings.Action,
                    ["label"] = settings.Label,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }, ex);
                throw;
            }
        }
    }
}
