using System;
using SimpleInjector;
using Take.Blip.Builder.Hosting;
using Take.Blip.Client.Activation;

namespace Take.Blip.Builder
{
    public class BuilderServiceProvider : Container, IServiceContainer
    {
        public BuilderServiceProvider()
        {
            this.RegisterBuilder();
        }

        public void RegisterService(Type serviceType, object instance)
        {
            RegisterSingleton(serviceType, instance);
        }

        public void RegisterService(Type serviceType, Func<object> instanceFactory)
        {
            RegisterSingleton(serviceType, instanceFactory);
        }
    }
}
