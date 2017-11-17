namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Defines a type that can be validated.
    /// </summary>
    public interface IValidable
    {
        /// <summary>
        /// Validate the current instance.
        /// </summary>
        void Validate();
    }
}