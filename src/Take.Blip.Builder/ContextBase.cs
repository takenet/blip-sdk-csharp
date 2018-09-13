using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    public abstract class ContextBase : IContext
    {
        public ContextBase(
            Identity user,
            LazyInput input,
            Flow flow)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Flow = flow ?? throw new ArgumentNullException(nameof(flow));
            InputContext = new Dictionary<string, object>();
        }

        public Identity User { get; }

        public LazyInput Input { get; }

        public Flow Flow { get; }

        public IDictionary<string, object> InputContext { get; }

        public abstract Task DeleteVariableAsync(string name, CancellationToken cancellationToken);

        public abstract Task<string> GetVariableAsync(string name, CancellationToken cancellationToken);

        public abstract Task SetVariableAsync(string name, string value, CancellationToken cancellationToken, TimeSpan expiration = default(TimeSpan));
    }
}
