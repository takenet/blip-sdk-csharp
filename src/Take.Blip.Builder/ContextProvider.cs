using System.Collections.Generic;
using Lime.Protocol;
using Take.Blip.Builder.Models;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Builder
{
    public class ContextProvider : IContextProvider
    {
        private readonly IBucketExtension _bucketExtension;

        public ContextProvider(IBucketExtension bucketExtension)
        {
            _bucketExtension = bucketExtension;
        }

        public IContext GetContext(Identity user, string flowId, IDictionary<string, string> flowVariables)
        {
            return new Context(_bucketExtension, user, flowId, flowVariables);
        }
    }
}