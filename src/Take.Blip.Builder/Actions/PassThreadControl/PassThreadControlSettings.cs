using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Actions.ThreadControl;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.PassThreadControl
{
    /// <summary>
    /// Represents the configuration settings for the Pass/Release Thread Control action within the Builder flow.
    /// </summary>
    public class PassThreadControlSettings : IValidable
    {
        /// <summary>
        /// Gets or sets the thread control operation type.
        /// Expected values for this action are <see cref="ThreadControlType.Pass"/> or <see cref="ThreadControlType.Release"/>.
        /// </summary>
        public ThreadControlType ControlType { get; set; }

        /// <summary>
        /// Gets or sets a legacy identity setting. Thread control commands always use
        /// the current contact identity from context; this setting does not override it.
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// Gets or sets the target partner role to route the conversation to.
        /// Required when <see cref="ControlType"/> is <see cref="ThreadControlType.Pass"/>; must be null for <see cref="ThreadControlType.Release"/>.
        /// </summary>
        public TargetRole? TargetRole { get; set; }

        /// <summary>
        /// Gets or sets optional contextual notes or metadata passed along with the handover.
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// Validates the settings instance ensuring required parameters match the specified control type.
        /// </summary>
        /// <exception cref="ValidationException">Thrown when validation rules are violated.</exception>
        public void Validate()
        {
            if (ControlType == ThreadControlType.Take)
            {
                throw new ValidationException(
                    $"The '{nameof(ControlType)}' value '{ThreadControlType.Take}' is not supported for '{nameof(PassThreadControlSettings)}'. Use the take control action instead.");
            }

            if (ControlType == ThreadControlType.Pass && TargetRole == null)
            {
                throw new ValidationException(
                    $"The '{nameof(TargetRole)}' setting value is required when '{nameof(ControlType)}' is '{ThreadControlType.Pass}'.");
            }

            if (ControlType == ThreadControlType.Release && TargetRole.HasValue)
            {
                throw new ValidationException(
                    $"The '{nameof(TargetRole)}' setting must not be set when '{nameof(ControlType)}' is '{ThreadControlType.Release}'.");
            }
        }
    }
}