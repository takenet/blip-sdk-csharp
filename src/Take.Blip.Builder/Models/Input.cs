using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Lime.Messaging.Contents;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Represents a input handling rules for a state.
    /// </summary>
    public class Input : IValidable
    {
        private static readonly Regex VariableValidationRegex = new Regex("^([a-zA-Z0-9\\.]+)$", RegexOptions.Compiled);

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

        public void Validate()
        {
            if (Validation != null)
            {
                if (Validation.Rule == InputValidationRule.Regex 
                    && string.IsNullOrWhiteSpace(Validation.Regex))
                {
                    throw new ValidationException("The regular expression should be provided when using the 'regex' validation rule");
                }

                if (string.IsNullOrWhiteSpace(Validation.Error))
                {
                    throw new ValidationException("The validation error message is required");
                }

                if (Validation.Rule == InputValidationRule.Type
                    && Validation.Type == null)
                {
                    throw new ValidationException("The media type should be provided when using the 'type' validation rule");
                }
            }

            if (!string.IsNullOrWhiteSpace(Variable) 
                && !VariableValidationRegex.IsMatch(Variable))
            {
                throw new ValidationException("The input variable name should be composed only by letters, numbers and dots");
            }
        }
    }
}