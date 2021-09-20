using System;
using System.Collections.Generic;
using System.Globalization;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.TrackEvent
{
    public class TrackEventSettings : IValidable
    {
        public string Category { get; set; }

        public string Action { get; set; }

        public string Label { get; set; }

        public string Value { get; set; }

        public decimal? ParsedValue
        {
            get
            {
                if (decimal.TryParse(Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue))
                {
                    return parsedValue;
                }

                return null;
            }
        }

        public IDictionary<string, string> Extras { get; set; }

        public bool? FireAndForget { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Category))
            {
                throw new ArgumentException($"The '{nameof(Category)}' settings value is required for '{nameof(TrackEventAction)}' action");
            }

            if (string.IsNullOrEmpty(Action))
            {
                throw new ArgumentException($"The '{nameof(Action)}' settings value is required for '{nameof(TrackEventAction)}' action");
            }
        }
    }
}
