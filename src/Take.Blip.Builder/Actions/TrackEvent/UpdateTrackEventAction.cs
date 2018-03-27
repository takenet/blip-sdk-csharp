using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Take.Blip.Client.Extensions.EventTracker;

namespace Take.Blip.Builder.Actions.TrackEvent
{
    public class UpdateTrackEventAction : IAction
    {
        private readonly IEventTrackExtension _eventTrackExtension;

        private const string MESSAGE_ID_KEY = "messageId";
        private const string INTENT_KEY = "intent";
        private const string NOT_HANDLED_KEY = "notHandled";
        private const string VERSION_KEY = "version";
        private const string SESSION_ID_KEY = "sessionId";

        public string Type => "UpdateTrackEvent";

        public UpdateTrackEventAction(IEventTrackExtension eventTrackExtension)
        {
            _eventTrackExtension = eventTrackExtension;
        }

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings), $"The settings are required for '{nameof(UpdateTrackEventAction)}' action");

            var messageId = (string)settings[MESSAGE_ID_KEY];
            var intent = (string)settings[INTENT_KEY];
            var notHandled = (string)settings[NOT_HANDLED_KEY];
            var version = (string)settings[VERSION_KEY];
            var sessionId = (string)settings[SESSION_ID_KEY];

            if (string.IsNullOrEmpty(messageId)) throw new ArgumentException($"The '{nameof(messageId)}' settings value is required for '{nameof(TrackEventAction)}' action");

            Dictionary<string, string> extras = null;
            if (settings.TryGetValue(nameof(extras), out var extrasToken))
            {
                extras = extrasToken.ToObject<Dictionary<string, string>>();
            }

            await _eventTrackExtension.UpdateMessageTrackAsync(messageId, extras, cancellationToken, context.User);
        }
    }
}
