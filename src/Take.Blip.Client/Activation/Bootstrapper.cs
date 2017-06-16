using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Listeners;
using Lime.Protocol.Server;
using Newtonsoft.Json;
using Take.Blip.Client.Extensions;
using Take.Blip.Client.Session;

namespace Take.Blip.Client.Activation
{
    public static class Bootstrapper
    {
        public const string DefaultApplicationFileName = "application.json";

        /// <summary>
        /// Creates and starts an application with the given settings.
        /// </summary>
        /// <param name="application">The application instance. If not defined, the class will look for an application.json file in the current directory.</param>
        /// <param name="builder">The builder instance to be used.</param>
        /// <param name="typeResolver">The resolver for type names.</param>
        public static async Task<IStoppable> StartAsync(CancellationToken cancellationToken, Application application = null, BlipClientBuilder builder = null, ITypeResolver typeResolver = null)
        {
            if (application == null)
            {
                if (!File.Exists(DefaultApplicationFileName))
                {
                    throw new FileNotFoundException($"Could not find the '{DefaultApplicationFileName}' file", DefaultApplicationFileName);
                }
                application = Application.ParseFromJsonFile(DefaultApplicationFileName);
            }

            if (builder == null) builder = new BlipClientBuilder();
            if (application.Identifier != null)
            {
                if (application.Password != null)
                {
                    builder = builder.UsingPassword(application.Identifier, application.Password);
                }
                else if (application.AccessKey != null)
                {
                    builder = builder.UsingAccessKey(application.Identifier, application.AccessKey);
                }
                else
                {
                    throw new ArgumentException("At least an access key or password must be defined", nameof(application));
                }
            }
            else
            {
                builder = builder.UsingGuest();
            }

            if (application.Instance != null) builder = builder.UsingInstance(application.Instance);
            if (application.RoutingRule != null) builder = builder.UsingRoutingRule(application.RoutingRule.Value);
            if (application.Domain != null) builder = builder.UsingDomain(application.Domain);
            if (application.Scheme != null) builder = builder.UsingScheme(application.Scheme);
            if (application.HostName != null) builder = builder.UsingHostName(application.HostName);
            if (application.Port != null) builder = builder.UsingPort(application.Port.Value);
            if (application.SendTimeout != 0) builder = builder.WithSendTimeout(TimeSpan.FromMilliseconds(application.SendTimeout));
            if (application.SessionEncryption.HasValue) builder = builder.UsingEncryption(application.SessionEncryption.Value);
            if (application.SessionCompression.HasValue) builder = builder.UsingCompression(application.SessionCompression.Value);
            if (application.Throughput != 0) builder = builder.WithThroughput(application.Throughput);
            if (application.DisableNotify) builder = builder.WithAutoNotify(false);
            if (application.ChannelCount.HasValue) builder = builder.WithChannelCount(application.ChannelCount.Value);
            if (application.ReceiptEvents != null && application.ReceiptEvents.Length > 0)
                builder = builder.WithReceiptEvents(application.ReceiptEvents);
            else if (application.ReceiptEvents != null)
                builder = builder.WithReceiptEvents(new[] { Event.Failed });

            if (typeResolver == null) typeResolver = TypeResolver.Instance;
            var localServiceProvider = BuildServiceProvider(application, typeResolver);

            localServiceProvider.RegisterService(typeof(BlipClientBuilder), builder);

            var client = await BuildClientAsync(application, builder.Build, localServiceProvider, typeResolver);

            var channelListener = BuildChannelListener(application);

            await client.StartAsync(channelListener, cancellationToken).ConfigureAwait(false);

            var stoppables = new IStoppable[2];
            stoppables[0] = client;
            var startable = await BuildStartupAsync(application, localServiceProvider, typeResolver);
            if (startable != null)
            {
                stoppables[1] = startable as IStoppable;
            }

            return new StoppableWrapper(stoppables);
        }
        public static IServiceContainer BuildServiceProvider(Application application, ITypeResolver typeResolver)
        {
            var localServiceProvider = new TypeServiceProvider();
            if (application.ServiceProviderType != null)
            {
                var serviceProviderType = typeResolver.Resolve(application.ServiceProviderType);
                if (serviceProviderType != null)
                {
                    if (!typeof(IServiceProvider).IsAssignableFrom(serviceProviderType))
                    {
                        throw new InvalidOperationException($"{application.ServiceProviderType} must be an implementation of '{nameof(IServiceProvider)}'");
                    }

                    if (serviceProviderType == typeof(TypeServiceProvider))
                    {
                        throw new InvalidOperationException($"{nameof(Application.ServiceProviderType)} type cannot be '{serviceProviderType.Name}'");
                    }

                    if (serviceProviderType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).All(c => c.GetParameters().Length != 0))
                    {
                        throw new InvalidOperationException($"{nameof(Application.ServiceProviderType)} must have an empty public constructor");
                    }

                    localServiceProvider.SecondaryServiceProvider = (IServiceProvider)Activator.CreateInstance(serviceProviderType);
                }
            }

            if (localServiceProvider.SecondaryServiceProvider is IServiceContainer serviceContainer)
            {
                return serviceContainer;
            }

            return localServiceProvider;
        }

        public static async Task<IStartable> BuildStartupAsync(Application application, IServiceContainer localServiceProvider, ITypeResolver typeResolver)
        {
            if (application.StartupType == null) return null;

            var startable = await CreateAsync<IStartable>(
                application.StartupType,
                localServiceProvider,
                application.Settings,
                typeResolver)
                .ConfigureAwait(false);
            await startable.StartAsync().ConfigureAwait(false);
            return startable;
        }

        public static async Task<IBlipClient> BuildClientAsync(
            Application application,
            Func<IBlipClient> builder,
            IServiceContainer serviceContainer,
            ITypeResolver typeResolver,
            Action<IServiceContainer> serviceOverrides = null)
        {
            RegisterSettingsContainer(application, serviceContainer, typeResolver);
            RegisterSettingsTypes(application, serviceContainer, typeResolver);

            serviceContainer.RegisterExtensions();
            serviceContainer.RegisterSession();
            serviceContainer.RegisterService(typeof(IServiceProvider), serviceContainer);
            serviceContainer.RegisterService(typeof(IServiceContainer), serviceContainer);
            serviceContainer.RegisterService(typeof(Application), application);

            var client = builder();
            serviceContainer.RegisterService(typeof(ISender), client);
            serviceOverrides?.Invoke(serviceContainer);

            var stateManager = serviceContainer.GetService<IStateManager>();
            var sessionManager = serviceContainer.GetService<ISessionManager>();

            //if (application.RegisterTunnelReceivers)
            //{
            //    RegisterTunnelReceivers(application);
            //}

            //await AddMessageReceivers(application, serviceContainer, client, typeResolver, stateManager, sessionManager);
            //await AddNotificationReceivers(application, serviceContainer, client, typeResolver, stateManager, sessionManager);
            //await AddCommandReceivers(application, serviceContainer, client, typeResolver);

            return client;
        }

        public static void RegisterSettingsContainer(SettingsContainer settingsContainer, IServiceContainer serviceContainer, ITypeResolver typeResolver)
        {
            if (settingsContainer.SettingsType != null)
            {
                var settingsDictionary = settingsContainer.Settings;
                var settingsType = typeResolver.Resolve(settingsContainer.SettingsType);
                if (settingsType != null)
                {
                    var settingsJson = JsonConvert.SerializeObject(settingsDictionary, Application.SerializerSettings);
                    var settings = JsonConvert.DeserializeObject(settingsJson, settingsType, Application.SerializerSettings);
                    serviceContainer.RegisterService(settingsType, settings);
                }
            }
        }

        public static void RegisterSettingsTypes(Application application, IServiceContainer serviceContainer, ITypeResolver typeResolver)
        {
            //var applicationReceivers =
            //    (application.MessageReceivers ?? new ApplicationReceiver[0]).Union(
            //        application.NotificationReceivers ?? new ApplicationReceiver[0]);

            //// First, register the receivers settings
            //foreach (var applicationReceiver in applicationReceivers.Where(a => a.SettingsType != null))
            //{
            //    RegisterSettingsContainer(applicationReceiver, serviceContainer, typeResolver);
            //}
        }

        private static IChannelListener BuildChannelListener(Application application)
        {
            return null;
        }

        public static Task<T> CreateAsync<T>(string typeName, IServiceProvider serviceProvider, IDictionary<string, object> settings, ITypeResolver typeResolver) where T : class
        {
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));
            var type = typeResolver.Resolve(typeName);
            return CreateAsync<T>(type, serviceProvider, settings);
        }

        public static async Task<T> CreateAsync<T>(Type type, IServiceProvider serviceProvider, IDictionary<string, object> settings) where T : class
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            IFactory<T> factory;
            if (typeof(IFactory<T>).IsAssignableFrom(type))
            {
                factory = (IFactory<T>)Activator.CreateInstance(type);
            }
            else
            {
                factory = new Factory<T>(type);
            }

            var instance = await factory.CreateAsync(serviceProvider, settings);
            if (instance == null)
            {
                throw new Exception($"{type.Name} does not implement {typeof(T).Name}");
            }
            return instance;
        }

        private class StoppableWrapper : IStoppable
        {
            private readonly IStoppable[] _stoppables;

            public StoppableWrapper(params IStoppable[] stoppables)
            {
                _stoppables = stoppables;
            }

            public async Task StopAsync(CancellationToken cancellationToken)
            {
                foreach (var stoppable in _stoppables)
                {
                    if (stoppable != null)
                        await stoppable.StopAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
