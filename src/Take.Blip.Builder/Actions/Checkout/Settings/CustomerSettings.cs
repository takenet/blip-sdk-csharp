using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.Checkout.Settings
{
    public class CustomerSettings : IValidable
    {
        public string Identity { get; set; }

        public string Phone { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Email { get; set; }

        public string DocumentType { get; set; }

        public string Document { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Identity))
                throw new ValidationException($"The 'Customer.{nameof(Identity)}' setting value is required for '{nameof(Checkout)}' action");

            if (string.IsNullOrWhiteSpace(Phone))
                throw new ValidationException($"The 'Customer.{nameof(Phone)}' setting value is required for '{nameof(Checkout)}' action");
        }
    }
}

