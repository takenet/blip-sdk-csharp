using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a context that queries directly in the storage engine.
    /// </summary>
    public class StorageContext : ContextBase
    {
        public StorageContext(Identity user, LazyInput input, Flow flow)
            : base(user, input, flow)
        {
        }

        public override Task<string> GetVariableAsync(string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task SetVariableAsync(string name, string value, CancellationToken cancellationToken, TimeSpan expiration = default(TimeSpan))
        {
            throw new NotImplementedException();
        }

        public override Task DeleteVariableAsync(string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
