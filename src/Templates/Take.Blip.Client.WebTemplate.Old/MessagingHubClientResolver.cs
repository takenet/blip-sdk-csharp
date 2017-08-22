using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using Takenet.MessagingHub.Client.Host;

namespace Take.Blip.Client.WebTemplate.Old
{
    internal class MessagingHubClientResolver : IDependencyResolver, IDependencyScope
    {
        private IServiceContainer _serviceContainer;

        public MessagingHubClientResolver(IServiceContainer serviceContainer)
        {
            _serviceContainer = serviceContainer;
        }

        public IDependencyScope BeginScope()
        {
            return this as IDependencyScope;
        }

        public void Dispose()
        {
            _serviceContainer.DisposeIfDisposable();
        }

        public object GetService(Type serviceType) => _serviceContainer.GetService(serviceType);

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var services = _serviceContainer.GetService(serviceType);
            return services as IEnumerable<object> ?? Enumerable.Empty<object>();
        }
    }
}