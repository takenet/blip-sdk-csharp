using System;
using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.SetVariable
{
    public class SetVariableSettings : IValidable
    {
        public string Variable { get; set; }

        public string Value { get; set; }

        public int? Expiration { get; set; }

        public void Validate()
        {
            if (Variable == null)
            {
                throw new ValidationException($"The '{nameof(Variable)}' settings value is required for '{nameof(SetVariable)}' action");
            }
        }
    }
}