using System;
using System.Collections.Generic;
using System.Text;
using Take.Blip.Client.Activation;

namespace Take.Blip.Client.TestKit
{
    internal class OverridableServiceContainer : TypeServiceProvider
    {
        public OverridableServiceContainer(IServiceProvider secondaryServiceProvider = null)
        {
            SecondaryServiceProvider = secondaryServiceProvider;
        }

        public override void RegisterService(Type serviceType, object instance)
        {
            TypeDictionary.Remove(serviceType);
            base.RegisterService(serviceType, instance);
        }

        public override void RegisterService(Type serviceType, Func<object> instanceFactory)
        {
            TypeDictionary.Remove(serviceType);
            base.RegisterService(serviceType, instanceFactory);
        }
    }
}
