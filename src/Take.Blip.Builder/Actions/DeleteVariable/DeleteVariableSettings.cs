using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.RemoveVariable
{
    public class DeleteVariableSettings : IValidable
    {
        public string Variable { get; set; }

        public void Validate()
        {
            if (Variable == null)
            {
                throw new ValidationException($"The '{nameof(Variable)}' setting value is required for '{nameof(RemoveVariable)}' action");
            }
        }
    }
}