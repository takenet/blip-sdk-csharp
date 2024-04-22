using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.ExecuteTemplate
{
    public class ExecuteTemplateSettings : IValidable
    {
        public string[] InputVariables { get; set; }

        public string OutputVariable { get; set; }
        
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