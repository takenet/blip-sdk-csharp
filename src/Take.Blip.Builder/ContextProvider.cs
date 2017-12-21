using System.Collections.Generic;
using Lime.Protocol;
using Take.Blip.Builder.Variables;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder
{
    public class ContextProvider : IContextProvider
    {
        private readonly IContextExtension _contextExtension;
        private readonly IEnumerable<IVariableProvider> _variableProviders;

        public ContextProvider(IContextExtension contextExtension, IEnumerable<IVariableProvider> variableProviders)
        {
            _contextExtension = contextExtension;
            _variableProviders = variableProviders;
        }

        public IContext GetContext(Identity user, string flowId) => new Context(flowId, user, _contextExtension, _variableProviders);
    }
}