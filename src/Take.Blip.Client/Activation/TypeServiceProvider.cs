using System;
using System.Collections.Generic;

namespace Take.Blip.Client.Activation
{
    /// <summary>
    /// Defines a simple type service provider.
    /// </summary>
    /// <seealso cref="Takenet.MessagingHub.Client.Host.IServiceContainer" />
    public class TypeServiceProvider : IServiceContainer
    {
        protected readonly Dictionary<Type, object> TypeDictionary;

        public TypeServiceProvider()
        {
            TypeDictionary = new Dictionary<Type, object>();
        }

        public IServiceProvider SecondaryServiceProvider { get; set; }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public virtual object GetService(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            object result;
            var service = TypeDictionary.TryGetValue(serviceType, out result) ?
                result : SecondaryServiceProvider?.GetService(serviceType);

            var factory = service as Func<object>;
            if (factory != null)
            {
                return factory();
            }

            return service;
        }

        /// <summary>
        /// Registers the service instance.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="instance">The instance.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public virtual void RegisterService(Type serviceType, object instance)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            TypeDictionary[serviceType] = instance ?? throw new ArgumentNullException(nameof(instance));
            (SecondaryServiceProvider as IServiceContainer)?.RegisterService(serviceType, instance);
        }

        public virtual void RegisterService(Type serviceType, Func<object> instanceFactory)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            TypeDictionary[serviceType] = instanceFactory ?? throw new ArgumentNullException(nameof(instanceFactory));
            (SecondaryServiceProvider as IServiceContainer)?.RegisterService(serviceType, instanceFactory);
        }
    }
}
