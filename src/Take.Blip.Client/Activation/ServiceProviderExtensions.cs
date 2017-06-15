using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Client.Activation
{
    public static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider serviceProvider) where T : class
        {
            var service = serviceProvider.GetService(typeof(T));
            var factory = service as Func<T>;
            if (factory != null)
            {
                return factory();
            }

            return service as T;
        }
    }
}
