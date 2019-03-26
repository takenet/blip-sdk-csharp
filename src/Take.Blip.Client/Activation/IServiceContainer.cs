using System;

namespace Take.Blip.Client.Activation
{
    /// <summary>
    /// Defines a service container that allows the registration of service type instances.
    /// </summary>
    /// <seealso cref="System.IServiceProvider" />
    public interface IServiceContainer : IServiceProvider
    {
        /// <summary>
        /// Registers the service instance.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="instance">The instance.</param>
        void RegisterService(Type serviceType, object instance);

        /// <summary>
        /// Registers a factory for the service instance.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="instanceFactory">
        /// A factory function which will be called
        /// only when a instance of <paramref name="serviceType"/> is requested.
        /// </param>
        void RegisterService(Type serviceType, Func<object> instanceFactory);

        /// <summary>
        /// Registers a factory for the decorator instance.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="instanceFactory">
        /// A factory function which will be called
        /// only when a instance of <paramref name="serviceType"/> is requested.
        /// </param>
        void RegisterDecorator(Type serviceType, Func<object> instanceFactory);
    }
}