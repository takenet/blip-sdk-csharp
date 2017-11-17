using System.ComponentModel.DataAnnotations;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Defines a conditions to be evaluated using the conversation context.
    /// </summary>
    public class Condition : IValidable
    {
        /// <summary>
        /// The source for comparison with the <see cref="Value"/>. Optional. The default value is <see cref="ValueSource.Input"/>.
        /// </summary>
        public ValueSource Source { get; set; }

        /// <summary>
        /// The variable name of the conversation context to be evaluated, if the <see cref="Source"/> value is <see cref="ValueSource.Context"/>.
        /// </summary>        
        public string Variable { get; set; }

        /// <summary>
        /// The type of the comparison. Optional. The default value is <see cref="ConditionComparison.Equals"/>.
        /// </summary>
        public ConditionComparison Comparison { get; set; }

        /// <summary>
        /// The value to be used by the comparison with the context value. Required.
        /// </summary>
        [Required(ErrorMessage = "The condition value is required")]
        public string Value { get; set; }

        public void Validate()
        {
            this.ValidateObject();

            if (Source == ValueSource.Context && string.IsNullOrWhiteSpace(Variable))
            {
                throw new ValidationException("The variable should be provided if the comparsion source is context");
            }
        }
    }
}
