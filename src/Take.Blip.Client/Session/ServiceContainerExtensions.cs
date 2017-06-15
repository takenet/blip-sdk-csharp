using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Client.Session
{
    public static class ServiceContainerExtensions
    {
        internal static IServiceContainer RegisterSession(this IServiceContainer serviceContainer)
        {
            serviceContainer.RegisterService(typeof(ISessionManager), () => new SessionManager(serviceContainer.GetService<IBucketExtension>()));
            serviceContainer.RegisterService(typeof(IStateManager), () => new StateManager(serviceContainer.GetService<IBucketExtension>()));
            return serviceContainer;
        }
    }
}
