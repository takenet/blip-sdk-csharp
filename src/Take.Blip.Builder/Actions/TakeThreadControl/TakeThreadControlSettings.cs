using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Take.Blip.Builder.Actions.ThreadControl;
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
        /// Gets or sets the target router service identifier to redirect the conversation to after taking control.
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// Gets or sets the optional payload or message sent alongside the service redirect.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Validates that a nonblank target service is configured for the redirect.
        /// </summary>
        /// <exception cref="ValidationException">Thrown when validation rules are violated.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Service))
            {
                throw new ValidationException(
                    $"The '{nameof(Service)}' setting value is required for '{nameof(TakeThreadControlSettings)}'.");
            }
        }
    }
}