using System;
using SimpleInjector;
using Take.Blip.Builder.Hosting;
using Take.Blip.Client.Activation;

namespace Builder.Console
{
    public class BuilderServiceProvider : Container, IServiceContainer
    {
        public BuilderServiceProvider()
        {
            Options.AllowOverridingRegistrations = true;
            this.RegisterBuilder();
            this.RegisterSingleton<IConfiguration, BuilderConfiguration>();
        }

        public void RegisterService(Type serviceType, object instance) => RegisterSingleton(serviceType, instance);

        public void RegisterService(Type serviceType, Func<object> instanceFactory) => RegisterSingleton(serviceType, instanceFactory);

        public void RegisterDecorator(Type serviceType, Func<object> instanceFactory) => RegisterDecorator(serviceType, instanceFactory);
    }
}