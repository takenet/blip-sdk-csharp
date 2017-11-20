using Lime.Messaging.Contents;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Represents a input handling rules for a state.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Indicates that the state input should be skipped.
        /// </summary>
        public bool Bypass { get; set; }

        /// <summary>
        /// Defines the validation rules for the input.
        /// </summary>
        public InputValidation Validation { get; set; }

        /// <summary>
        /// The context variable name to store the input after validation.
        /// </summary>
        public string Variable { get; set; }
    }
}