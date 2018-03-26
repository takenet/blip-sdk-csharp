using System;
using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.ExecuteScript
{
    public class ExecuteScriptSettings : IValidable
    {
        public string Function { get; set; }

        public string Source { get; set; }

        public string[] InputVariables { get; set; }

        public string OutputVariable { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Source))
            {
                throw new ValidationException($"The '{nameof(Source)}' settings value is required for '{nameof(ExecuteScriptSettings)}' action");
            }

            if (string.IsNullOrEmpty(OutputVariable))
            {
                throw new ValidationException($"The '{nameof(OutputVariable)}' settings value is required for '{nameof(ExecuteScriptSettings)}' action");
            }
        }
    }
}
