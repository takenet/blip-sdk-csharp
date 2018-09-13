using System.Collections.Generic;
using Lime.Protocol;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Storage;
using Take.Blip.Builder.Variables;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a context 
    /// </summary>
    public class ContextProvider : IContextProvider
    {
        private readonly IConfiguration _configuration;         
        private readonly IEnumerable<IVariableProvider> _variableProviders;
        private readonly IContextExtension _contextExtension;
        private readonly IOwnerCallerNameDocumentMap _ownerCallerNameDocumentMap;


        public ContextProvider(
            IConfiguration configuration,            
            IEnumerable<IVariableProvider> variableProviders,
            IContextExtension contextExtension,
            IOwnerCallerNameDocumentMap ownerCallerNameDocumentMap)
        {
            _contextExtension = contextExtension;            
            _configuration = configuration;
            _variableProviders = variableProviders;
            _ownerCallerNameDocumentMap = ownerCallerNameDocumentMap;
        }

        public IContext CreateContext(Identity user, Identity application, LazyInput input, Flow flow)
        {
            switch (_configuration.ContextType)
            {
                case nameof(StorageContext):
                    return new StorageContext(user, application, input, flow, _variableProviders, _ownerCallerNameDocumentMap);

                default:
                    return new ExtensionContext(user, application, input, flow, _variableProviders, _contextExtension);
            }
        }
    }
}