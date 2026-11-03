using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.TakeThreadControl
{
    /// <summary>
    /// Represents the configuration settings for the Take Thread Control action within the Builder flow.
    /// </summary>
    public class TakeThreadControlSettings : IValidable
    {
        /// <summary>
        /// Gets or sets optional contextual notes or metadata regarding the takeover.
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// Validates the settings instance ensuring prohibited fields are absent and mutual dependencies are met.
        /// </summary>
        public void Validate()
        {
            // No required settings for take thread control.
        }
    }
}