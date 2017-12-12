using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Client.Extensions.EventTracker;

namespace Take.Blip.Builder.Actions
{
    public class EventTrackAction : IAction
    {
        private readonly IEventTrackExtension _eventTrackExtension;

        private const string CATEGORY_KEY = "category";
        private const string ACTION_KEY = "action";
        
        public string Type => "EventTrack";

        public EventTrackAction(IEventTrackExtension eventTrackExtension)
        {
            _eventTrackExtension = eventTrackExtension;
        }

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings), $"The settings are required for '{nameof(EventTrackAction)}' action");

            var category = (string)settings[CATEGORY_KEY];
            var action = (string)settings[ACTION_KEY];

            if (string.IsNullOrEmpty(category)) throw new ArgumentException($"The '{nameof(category)}' settings value is required for '{nameof(EventTrackAction)}' action");
            if (string.IsNullOrEmpty(action)) throw new ArgumentException($"The '{nameof(action)}' settings value is required for '{nameof(EventTrackAction)}' action");


            Identity identity = null;

            if (settings.TryGetValue(nameof(identity), out var categoryToken))
            {
                identity = categoryToken.ToObject<string>();
            }

            Dictionary<string, string> extras = null;
            if (settings.TryGetValue(nameof(extras), out var extrasToken))
            {
                extras = extrasToken.ToObject<Dictionary<string, string>>();
            }

            await _eventTrackExtension.AddAsync(category, action, extras, cancellationToken, identity);
        }
    }
}
