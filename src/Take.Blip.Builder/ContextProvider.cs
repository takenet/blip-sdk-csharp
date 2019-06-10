using System;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly Lazy<IEnumerable<IVariableProvider>> _variableProviders;
        private readonly Lazy<IContextExtension> _contextExtension;
        private readonly Lazy<IOwnerCallerNameDocumentMap> _ownerCallerNameDocumentMap;

        public ContextProvider(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _contextExtension = new Lazy<IContextExtension>(() => _serviceProvider.GetService<IContextExtension>());
            _variableProviders = new Lazy<IEnumerable<IVariableProvider>>(() => _serviceProvider.GetService<IEnumerable<IVariableProvider>>());
            _ownerCallerNameDocumentMap = new Lazy<IOwnerCallerNameDocumentMap>(() => _serviceProvider.GetService<IOwnerCallerNameDocumentMap>());
        }

        public IContext CreateContext(Identity userIdentity, Identity ownerIdentity, LazyInput input, Flow flow)
        {
            switch (_configuration.ContextType)
            {
                case nameof(StorageContext):
                    return new StorageContext(userIdentity, ownerIdentity, input, flow, _variableProviders.Value, _ownerCallerNameDocumentMap.Value);

                default:
                    return new ExtensionContext(userIdentity, ownerIdentity, input, flow, _variableProviders.Value, _contextExtension.Value);
            }
        }
    }

    public static class ServiceProviderExtensions
    {
        public static TService GetService<TService>(this IServiceProvider serviceProvider)
        {
            return (TService)serviceProvider.GetService(typeof(TService));
        }
    }
}