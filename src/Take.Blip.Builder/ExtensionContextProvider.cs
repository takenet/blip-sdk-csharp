using System.Collections.Generic;
using Lime.Protocol;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Variables;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a context 
    /// </summary>
    public class ExtensionContextProvider : IContextProvider
    {
        private readonly IContextExtension _contextExtension;
        private readonly IEnumerable<IVariableProvider> _variableProviders;

        public ExtensionContextProvider(IContextExtension contextExtension, IEnumerable<IVariableProvider> variableProviders)
        {
            _contextExtension = contextExtension;
            _variableProviders = variableProviders;
        }

        public IContext CreateContext(Identity user, LazyInput input, Flow flow) => new ExtensionContext(user, input, flow, _contextExtension, _variableProviders);
    }
}