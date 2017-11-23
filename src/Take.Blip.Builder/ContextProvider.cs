using System.Collections.Generic;
using Lime.Protocol;
using Take.Blip.Builder.Models;
using Take.Blip.Client.Extensions.Bucket;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder
{
    public class ContextProvider : IContextProvider
    {
        private readonly IContextExtension _contextExtension;

        public ContextProvider(IContextExtension contextExtension)
        {
            _contextExtension = contextExtension;
        }

        public IContext GetContext(Identity user, string flowId, IDictionary<string, string> flowVariables)
        {
            return new Context(_contextExtension, flowId, user, flowVariables);
        }
    }
}