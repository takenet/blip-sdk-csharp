using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Actions.ThreadControl;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.PassThreadControl
{
    public class PassThreadControlSettings : IValidable
    {
        public string TargetRole { get; set; }

        public string Metadata { get; set; }

        public void Validate()
        {
            if (TargetRole != null && !ThreadControlConstants.AllowedTargetRoles.Contains(TargetRole))
            {
                throw new ValidationException(
                    $"The '{nameof(TargetRole)}' setting value is invalid for '{nameof(PassThreadControl)}' action");
            }
        }
    }
}
