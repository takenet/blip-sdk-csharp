using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Serialization.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.EventTracker;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder.Actions
{
    public class EventTrackAction : IAction
    {
        private readonly IEventTrackExtension _eventTrackExtension;
        private readonly JsonSerializer _serializer;

        private const string CATEGORY_KEY = "category";
        private const string ACTION_KEY = "action";
        private const string IDENTITY_KEY = "identity";
        private const string EXTRAS_KEY = "extras";

        public string Type => "EventTrack";

        public EventTrackAction(IEventTrackExtension eventTrackExtension)
        {
            _eventTrackExtension = eventTrackExtension;
            _serializer = JsonSerializer.Create(JsonNetSerializer.Settings);
        }

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings), "The settings are required for 'SendMessage' action");

            var category = (string)settings[CATEGORY_KEY];
            var action = (string)settings[ACTION_KEY];
            var identity = (string)settings[IDENTITY_KEY];
            var extras = settings[EXTRAS_KEY].ToObject<Dictionary<string, string>>();

            await _eventTrackExtension.AddAsync(category, action, extras, cancellationToken, identity);
        }
    }
}
