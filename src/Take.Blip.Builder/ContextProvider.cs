using System;
using System.Collections.Generic;
using Lime.Protocol;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly Lazy<IEnumerable<IVariableProvider>> _variableProviders;
        private readonly Lazy<IContextExtension> _contextExtension;

        public ContextProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _contextExtension = new Lazy<IContextExtension>(() => _serviceProvider.GetService<IContextExtension>());
            _variableProviders = new Lazy<IEnumerable<IVariableProvider>>(() => _serviceProvider.GetService<IEnumerable<IVariableProvider>>());
        }

        public IContext CreateContext(Identity userIdentity, Identity ownerIdentity, LazyInput input, Flow flow)
        {
            return new ExtensionContext(userIdentity, ownerIdentity, input, flow, _variableProviders.Value, _contextExtension.Value);
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