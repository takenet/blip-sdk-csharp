using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            if (Comparison.GetComparisonType() == ComparisonType.Unary && Values != null && Values.Length > 0)
            {
                throw new ValidationException("The condition values should not be provided if comparison is Exists or NotExists");
            }

            if (Comparison.GetComparisonType() == ComparisonType.Binary && (Values == null || Values.Length == 0))
            {
                throw new ValidationException("The condition values should be provided if comparison is not Exists or NotExists");
            }
        }
        
        public async Task<bool> EvaluateConditionAsync(
            LazyInput lazyInput,
            IContext context,
            CancellationToken cancellationToken)
        {
            string comparisonValue;

            switch (Source)
            {
                case ValueSource.Input:
                    comparisonValue = lazyInput.SerializedContent;
                    break;

                case ValueSource.Context:
                    comparisonValue = await context.GetVariableAsync(Variable, cancellationToken);
                    break;

                case ValueSource.Intent:
                    comparisonValue = (await lazyInput.GetIntentAsync())?.Name;
                    break;

                case ValueSource.Entity:
                    comparisonValue = (await lazyInput.GetEntityValue(Entity))?.Value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (Comparison.GetComparisonType())
            {
                case ComparisonType.Unary:
                    var unaryComparisonFunc = Comparison.ToUnaryDelegate();

                    return unaryComparisonFunc(comparisonValue);

                case ComparisonType.Binary:
                    var binaryComparisonFunc = Comparison.ToBinaryDelegate();

                    switch (Operator)
                    {
                        case ConditionOperator.Or:
                            return Values.Any(v => binaryComparisonFunc(comparisonValue, v));

                        case ConditionOperator.And:
                            return Values.All(v => binaryComparisonFunc(comparisonValue, v));

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }        
    }
}
