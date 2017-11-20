using System.ComponentModel.DataAnnotations;

namespace Take.Blip.Builder.Models
{
    public static class ObjectExtensions
    {
        public static void ValidateObject(this object value)
        {
            Validator.ValidateObject(value, new ValidationContext(value), true);
        }
    }
}