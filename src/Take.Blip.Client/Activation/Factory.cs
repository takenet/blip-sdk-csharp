using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Take.Blip.Client.Activation
{
    internal class Factory<T> : IFactory<T> where T : class
    {
        private readonly Type _type;

        public Factory(Type type)
        {
            if (type.IsAssignableFrom(typeof(T))) throw new ArgumentException($"The type '{type}' is not assignable from '{typeof(T)}'");
            _type = type;
        }

        public Task<T> CreateAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings)
        {
            T service;
            try
            {
                service = serviceProvider.GetService(_type) as T;
            }
            catch (Exception)
            {
                service = null;
            }

            if (service == null)
            {
                service = GetService(_type, serviceProvider, settings) as T;
            }

            return Task.FromResult(service);
        }

        private static object GetService(Type serviceType, IServiceProvider serviceProvider, params object[] args)
        {
            // Check the type constructors
            try
            {
                var serviceConstructor = serviceType
                    .GetConstructors()
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault();

                if (serviceConstructor == null)
                {
                    throw new ArgumentException($"The  type '{serviceType}' doesn't have a public constructor", nameof(serviceType));
                }

                var parameters = serviceConstructor.GetParameters();
                var serviceArgs = new object[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];

                    var arg = args.FirstOrDefault(p => parameter.ParameterType.IsInstanceOfType(p));
                    if (arg != null)
                    {
                        serviceArgs[i] = arg;
                    }
                    else
                    {
                        serviceArgs[i] = serviceProvider.GetService(parameter.ParameterType);
                    }
                }

                return Activator.CreateInstance(serviceType, serviceArgs);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Could not instantiate type {serviceType.FullName}", ex);
            }
        }
    }
}
