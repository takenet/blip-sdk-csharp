using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Actions.ExecuteScript;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.ExecuteScriptV2
{
    /// <summary>
    /// Settings for the ExecuteScriptV2 action.
    /// </summary>
    public class ExecuteScriptV2Settings : IValidable
    {
        /// <summary>
        /// The function to call in the script.
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// The script source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The input variables to pass to the script.
        /// </summary>
        public string[] InputVariables { get; set; }

        /// <summary>
        /// The output variable to store the result of the script.
        /// </summary>
        public string OutputVariable { get; set; }

        /// <summary>
        /// If the script should capture all exceptions instead of throwing them.
        /// </summary>
        public bool CaptureExceptions { get; set; }

        /// <summary>
        /// The variable to store the exception message if CaptureExceptions is true.
        /// </summary>
        public string ExceptionVariable { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool LocalTimeZoneEnabled { get; set; }

        /// <summary>
        /// The current state id to send as header of the request.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string currentStateId { get; set; }

        /// <inheritdoc />
        public void Validate()
        {
            if (string.IsNullOrEmpty(Source))
            {
                throw new ValidationException(
                    $"The '{nameof(Source)}' settings value is required for '{nameof(ExecuteScriptSettings)}' action");
            }

            if (string.IsNullOrEmpty(OutputVariable))
            {
                throw new ValidationException(
                    $"The '{nameof(OutputVariable)}' settings value is required for '{nameof(ExecuteScriptSettings)}' action");
            }
        }
    }
}