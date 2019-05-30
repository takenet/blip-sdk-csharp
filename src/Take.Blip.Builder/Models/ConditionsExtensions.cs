using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Models
{
    public static class ConditionsExtensions
    {
        public static async Task<bool> EvaluateConditionsAsync(
           this IEnumerable<Condition> conditions,
           LazyInput lazyInput,
           IContext context,
           CancellationToken cancellationToken)
        {
            foreach (var outputCondition in conditions)
            {
                if (!await outputCondition.EvaluateConditionAsync(lazyInput, context, cancellationToken))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
