using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Models
{
    public static class ConditionExtensions
    {
        public static async Task<bool> EvaluateConditionAsync(
            this Condition condition,
            LazyInput lazyInput,
            IContext context,
            CancellationToken cancellationToken)
        {
            string comparisonValue;

            switch (condition.Source)
            {
                case ValueSource.Input:
                    comparisonValue = lazyInput.SerializedContent;
                    break;

                case ValueSource.Context:
                    comparisonValue = await context.GetVariableAsync(condition.Variable, cancellationToken);
                    break;

                case ValueSource.Intent:
                    comparisonValue = (await lazyInput.GetIntentAsync())?.Name;
                    break;

                case ValueSource.Entity:
                    comparisonValue = (await lazyInput.GetEntityValue(condition.Entity))?.Value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (condition.Comparison.GetComparisonType())
            {
                case ComparisonType.Unary:
                    var unaryComparisonFunc = condition.Comparison.ToUnaryDelegate();

                    return unaryComparisonFunc(comparisonValue);

                case ComparisonType.Binary:
                    var binaryComparisonFunc = condition.Comparison.ToBinaryDelegate();

                    switch (condition.Operator)
                    {
                        case ConditionOperator.Or:
                            return condition.Values.Any(v => binaryComparisonFunc(comparisonValue, v));

                        case ConditionOperator.And:
                            return condition.Values.All(v => binaryComparisonFunc(comparisonValue, v));

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
