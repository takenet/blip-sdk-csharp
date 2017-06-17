using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Activation.Rules
{

    public abstract class ConditionBase : NamedEntity
    {
        public ConditionBase(string name)
            : base(name)
        {
            
        }      

        public abstract Task<bool> IsMatchAsync(object factProperty, IDictionary<string, object> context, CancellationToken cancellationToken);

    }
}
