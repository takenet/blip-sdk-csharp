using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.EventTracker;

namespace Take.Blip.Builder.Actions.TrackEvent
{
    public class TrackEventAction : ActionBase<TrackEventSettings>
    {
        private readonly IEventTrackExtension _eventTrackExtension;

        public TrackEventAction(IEventTrackExtension eventTrackExtension)
            : base(nameof(TrackEvent))
        {
            _eventTrackExtension = eventTrackExtension;
        }

        public override Task ExecuteAsync(IContext context, TrackEventSettings settings, CancellationToken cancellationToken)
        {
            return _eventTrackExtension.AddAsync(
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
        }
    }
}
