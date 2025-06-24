using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.Checkout.Settings
{
    public class ProductSettings : IValidable
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Description))
                throw new ValidationException($"The 'Product.{nameof(Description)}' setting value is required for '{nameof(Checkout)}' action");
            if (Quantity <= 0)
                throw new ValidationException($"The 'Product.{nameof(Quantity)}' setting value must be greater than zero for '{nameof(Checkout)}' action");
            if (Price < 0)
                throw new ValidationException($"The 'Product.{nameof(Price)}' setting value cannot be negative for '{nameof(Checkout)}' action");
        }
    }
}
