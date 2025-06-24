using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.Checkout.Settings
{
    public class CheckoutSettings : IValidable
    {
        public CustomerSettings Customer { get; set; }
        public ProductSettings[] Products { get; set; }
        public string Currency { get; set; }
        public string Channel { get; set; }
        public string Source { get; set; }

        public void Validate()
        {
            if (Customer == null)
                throw new ValidationException($"The '{nameof(Customer)}' setting value is required for '{nameof(Checkout)}' action");

            if (Products == null)
                throw new ValidationException($"The '{nameof(Products)}' setting value is required for '{nameof(Checkout)}' action");

            if (Currency == null)
                throw new ValidationException($"The '{nameof(Currency)}' setting value is required for '{nameof(Checkout)}' action");

            if (Channel == null)
                throw new ValidationException($"The '{nameof(Channel)}' setting value is required for '{nameof(Checkout)}' action");

            if (Source == null)
                throw new ValidationException($"The '{nameof(Source)}' setting value is required for '{nameof(Source)}' action");

        }

        


    }
}