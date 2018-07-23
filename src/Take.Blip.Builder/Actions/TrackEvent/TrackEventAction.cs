using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.EventTracker;

namespace Take.Blip.Builder.Actions.TrackEvent
{
    public class TrackEventAction : IAction
    {
        private readonly IEventTrackExtension _eventTrackExtension;

        private const string CATEGORY_KEY = "category";
        private const string ACTION_KEY = "action";
        private const string LABEL_KEY = "label";
        private const string VALUE_KEY = "value";

        public string Type => nameof(TrackEvent);

        public TrackEventAction(IEventTrackExtension eventTrackExtension)
        {
            _eventTrackExtension = eventTrackExtension;
        }

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings), $"The settings are required for '{nameof(TrackEventAction)}' action");

            var category = (string)settings[CATEGORY_KEY];
            var action = (string)settings[ACTION_KEY];

            if (string.IsNullOrEmpty(category)) throw new ArgumentException($"The '{nameof(category)}' settings value is required for '{nameof(TrackEventAction)}' action");
            if (string.IsNullOrEmpty(action)) throw new ArgumentException($"The '{nameof(action)}' settings value is required for '{nameof(TrackEventAction)}' action");

            var messageId = EnvelopeReceiverContext<Message>.Envelope?.Id;

            Dictionary<string, string> extras;
            if (settings.TryGetValue(nameof(extras), out var extrasToken))
            {
                extras = extrasToken.ToObject<Dictionary<string, string>>();
            }
            else
            {
                extras = new Dictionary<string, string>();
            }

            var valueString = (string)settings[VALUE_KEY];
            decimal? value = null;
            if (!string.IsNullOrEmpty(valueString))
            {
                decimal parsedValue;
                if(!decimal.TryParse(valueString, out parsedValue))
                {
                    throw new ArgumentException($"The '{nameof(value)}' settings could not be parsed to decimal in the '{nameof(TrackEventAction)}' action");
                }
                value = parsedValue;
            }

            await _eventTrackExtension.AddAsync(category, action, label:(string)settings[LABEL_KEY], value: value, messageId: messageId, extras: extras, cancellationToken: cancellationToken, contactIdentity: context.User);
        }
    }
}
