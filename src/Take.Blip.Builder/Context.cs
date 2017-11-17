using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Builder
{
    public class Context : IContext
    {
        private readonly IBucketExtension _bucketExtension;
        private readonly IDictionary<string, string> _flowVariables;

        public Context(IBucketExtension bucketExtension, Identity user, string flowId, IDictionary<string, string> flowVariables)
        {
            _bucketExtension = bucketExtension;
            _flowVariables = flowVariables;
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public string FlowId { get; set; }

        public Identity User { get; }

        public Task SetVariableAsync(string name, string value, CancellationToken cancellationToken) 
            => _bucketExtension.SetAsync(BucketIdHelper.GetId(FlowId, User, name), value, cancellationToken);

        public Task<string> GetVariableAsync(string name, CancellationToken cancellationToken)
        {
            if (_flowVariables != null && 
                _flowVariables.TryGetValue(name, out var variableValue))
            {
                return Task.FromResult(variableValue);
            }

            return _bucketExtension.GetAsync(BucketIdHelper.GetId(FlowId, User, name), cancellationToken);
        }
    }
}