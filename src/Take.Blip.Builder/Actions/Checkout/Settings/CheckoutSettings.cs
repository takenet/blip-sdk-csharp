using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.Checkout.Settings
{
    public class CheckoutSettings : IValidable
    {
        public CustomerSettings Customer { get; set; }

        public ProductSettings[] Products { get; set; }

        public string Merchant { get; set; }

        public string Currency { get; set; }

        public string Channel { get; set; }

        public string Source { get; set; }

        public string Message { get; set; }

        public string ReminderContent { get; set; }

        public void Validate()
        {
            if (Merchant == null)
                throw new ValidationException($"The '{nameof(Merchant)}' setting value is required for '{nameof(Checkout)}' action");

            if (Customer == null)
                throw new ValidationException($"The '{nameof(Customer)}' setting value is required for '{nameof(Checkout)}' action");

            if (Products == null)
                throw new ValidationException($"The '{nameof(Products)}' setting value is required for '{nameof(Checkout)}' action");

            if (Currency == null)
                throw new ValidationException($"The '{nameof(Currency)}' setting value is required for '{nameof(Checkout)}' action");

            if (Channel == null)
                throw new ValidationException($"The '{nameof(Channel)}' setting value is required for '{nameof(Checkout)}' action");

            if (Source == null)
                throw new ValidationException($"The '{nameof(Source)}' setting value is required for '{nameof(Checkout)}' action");

            if (Message == null)
                throw new ValidationException($"The '{nameof(Message)}' setting value is required for '{nameof(Checkout)}' action");

            if (ReminderContent == null)
                throw new ValidationException($"The '{nameof(ReminderContent)}' setting value is required for '{nameof(Checkout)}' action");

        }
    }
}