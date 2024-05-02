using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.ExecuteTemplate
{
    /// <summary>
    /// Settings to Execute Template Action
    /// </summary>
    public class ExecuteTemplateSettings : IValidable
    {
        /// <summary>
        /// Input Variables
        /// </summary>
        public string[] InputVariables { get; set; }

        /// <summary>
        /// Output Variable
        /// </summary>
        public string OutputVariable { get; set; }
        
        /// <summary>
        /// Template that will be transformed
        /// </summary>
        public string Template { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(OutputVariable))
            {
                throw new ValidationException($"The '{nameof(OutputVariable)}' settings value is required for '{nameof(ExecuteTemplateSettings)}' action");
            }
        }
    }
}