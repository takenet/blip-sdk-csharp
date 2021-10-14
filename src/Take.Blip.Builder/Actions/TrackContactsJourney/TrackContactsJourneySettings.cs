using System;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.TrackContactsJourney
{
    public class TrackContactsJourneySettings : IValidable
    {
        public string StateId { get; set; }

        public string StateName { get; set; }

        public string PreviousStateId { get; set; }

        public string PreviousStateName { get; set; }

        public bool? FireAndForget { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(StateId))
            {
                throw new ArgumentException($"The '{nameof(StateId)}' settings value is required for '{nameof(TrackContactsJourneyAction)}' action");
            }

            if (string.IsNullOrEmpty(StateName))
            {
                throw new ArgumentException($"The '{nameof(StateName)}' settings value is required for '{nameof(TrackContactsJourneyAction)}' action");
            }
        }
    }
}
