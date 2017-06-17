using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Activation.Rules
{

    public abstract class ActionBase : NamedEntity
    {
        public ActionBase(string name)
            : base(name)
        {

        }

        public abstract Task ExecuteAsync(IDictionary<string, object> fact, IDictionary<string, object> context, CancellationToken cancellationToken);
    }
}
