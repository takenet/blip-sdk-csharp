using System.ComponentModel.DataAnnotations;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Defines a conditions to be evaluated using the conversation context.
    /// </summary>
    public class Condition : IValidable
    {
        /// <summary>
        /// The source for comparison with the <see cref="ValueSource"/>. Optional. The default value is <see cref="ValueSource.Input"/>.
        /// </summary>
        public ValueSource Source { get; set; }

        /// <summary>
        /// The variable name of the conversation context to be evaluated, if the <see cref="Source"/> value is <see cref="ValueSource.Context"/>.
        /// </summary>        
        public string Variable { get; set; }

        /// <summary>
        /// The name of the entity to be evaluated, if the <see cref="Source"/> value is <see cref="ValueSource.Entity"/>.
        /// </summary>
        public string Entity { get; set; }

        /// <summary>
        /// The type of the comparison. Optional. The default value is <see cref="ConditionComparison.Equals"/>.
        /// </summary>
        public ConditionComparison Comparison { get; set; }

        /// <summary>
        /// The operator for comparison with the provided values. The default value is <see cref="ConditionOperator.Or"/>.
        /// </summary>
        public ConditionOperator Operator { get; set; }

        /// <summary>
        /// The values to be used by the comparison with the context value.
        /// </summary>
        public string[] Values { get; set; }
               
        public void Validate()
        {
            this.ValidateObject();

            if (Source == ValueSource.Context && string.IsNullOrWhiteSpace(Variable))
            {
                throw new ValidationException("The variable name should be provided if the comparsion source is context");
            }

            if (Source == ValueSource.Entity && string.IsNullOrWhiteSpace(Entity))
            {
                throw new ValidationException("The entity name should be provided if the comparsion source is entity");
            }

            if ((Comparison == ConditionComparison.Exists || Comparison == ConditionComparison.NotExists) && (Values != null && Values.Length >= 0))
            {
                throw new ValidationException("The condition values should not be provided if comparison is Exists or NotExists");
            }

            if ( (Comparison == ConditionComparison.Equals 
               || Comparison == ConditionComparison.NotEquals
               || Comparison == ConditionComparison.Contains
               || Comparison == ConditionComparison.StartsWith
               || Comparison == ConditionComparison.EndsWith
               || Comparison == ConditionComparison.GreaterThan
               || Comparison == ConditionComparison.LessThan
               || Comparison == ConditionComparison.GreaterThanOrEquals
               || Comparison == ConditionComparison.LessThanOrEquals
               || Comparison == ConditionComparison.Matches
               || Comparison == ConditionComparison.ApproximateTo) 
               && (Values == null ||Values.Length == 0))
            {
                throw new ValidationException("The condition values should be provided if comparison is not Exists or NotExists");
            }
        }
    }
}
