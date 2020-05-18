using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Server;
using Newtonsoft.Json;
using Serilog;
using Take.Blip.Client.Extensions;
using Take.Blip.Client.Extensions.Bucket;
using Take.Blip.Client.Extensions.Tunnel;
using Take.Blip.Client.Receivers;
using Take.Blip.Client.Session;

namespace Take.Blip.Client.Activation
{
    public static class Bootstrapper
    {
        public const string DefaultApplicationFileName = "application.json";

        /// <summary>
        /// Creates and starts an application with the given settings.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="application">The application instance. If not defined, the class will look for an application.json file in the current directory.</param>
        /// <param name="builder">The builder instance to be used.</param>
        /// <param name="typeResolver">The resolver for type names.</param>
        /// <param name="logger">The logger instance to be used.</param>
        public static async Task<IStoppable> StartAsync(
            CancellationToken cancellationToken,
            Application application = null,
            BlipClientBuilder builder = null,
            ITypeResolver typeResolver = null,
            ILogger logger = null)
        {
            if (application == null)
            {
                if (!File.Exists(DefaultApplicationFileName))
                {
                    throw new FileNotFoundException($"Could not find the '{DefaultApplicationFileName}' file", DefaultApplicationFileName);
                }
                application = Application.ParseFromJsonFile(DefaultApplicationFileName);
            }

            if (builder == null) builder = new BlipClientBuilder(new TcpTransportFactory(), logger);
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
            {
                builder = builder.WithReceiptEvents(application.ReceiptEvents);
            }
            else if (application.ReceiptEvents != null)
            {
                builder = builder.WithReceiptEvents(new[] { Event.Failed });
            }
            if (application.PresenceStatus.HasValue) builder = builder.WithPresenceStatus(application.PresenceStatus.Value);

            if (application.EnvelopeBufferSize.HasValue) builder = builder.WithEnvelopeBufferSize(application.EnvelopeBufferSize.Value);

            if (typeResolver == null) typeResolver = new TypeResolver();
            var localServiceProvider = BuildServiceProvider(application, typeResolver);

            localServiceProvider.RegisterService(typeof(BlipClientBuilder), builder);

            var client = await BuildClientAsync(application, builder.Build, localServiceProvider, typeResolver, cancellationToken, logger: logger);

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
            CancellationToken cancellationToken,
            Action<IServiceContainer> serviceOverrides = null,
            ILogger logger = null)
        {
            RegisterSettingsContainer(application, serviceContainer, typeResolver);
            RegisterSettingsTypes(application, serviceContainer, typeResolver);
            RegisterStateManager(application, serviceContainer, typeResolver);

            serviceContainer.RegisterExtensions();
            serviceContainer.RegisterService(typeof(IServiceProvider), serviceContainer);
            serviceContainer.RegisterService(typeof(IServiceContainer), serviceContainer);
            serviceContainer.RegisterService(typeof(Application), application);
            serviceContainer.RegisterService(typeof(ISessionManager), () => new SessionManager(serviceContainer.GetService<IBucketExtension>()));

            if (logger != null) serviceContainer.RegisterService(typeof(ILogger), logger);
            
            var client = builder();
            serviceContainer.RegisterService(typeof(ISender), client);
            serviceOverrides?.Invoke(serviceContainer);

            var stateManager = serviceContainer.GetService<IStateManager>();
            var sessionManager = serviceContainer.GetService<ISessionManager>();

            if (application.RegisterTunnelReceivers)
            {
                RegisterTunnelReceivers(application);
            }

            var channelListener = new BlipChannelListener(client, !application.DisableNotify, logger);

            await AddMessageReceivers(application, serviceContainer, client, channelListener, typeResolver, stateManager, sessionManager);
            await AddNotificationReceivers(application, serviceContainer, client, channelListener, typeResolver, stateManager, sessionManager);
            await AddCommandReceivers(application, serviceContainer, channelListener, typeResolver);

            await client.StartAsync(channelListener, cancellationToken).ConfigureAwait(false);

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
            var applicationReceivers =
                (application.MessageReceivers ?? new ApplicationReceiver[0]).Union(
                    application.NotificationReceivers ?? new ApplicationReceiver[0]);

            // First, register the receivers settings
            foreach (var applicationReceiver in applicationReceivers.Where(a => a.SettingsType != null))
            {
                RegisterSettingsContainer(applicationReceiver, serviceContainer, typeResolver);
            }
        }

        public static void RegisterStateManager(Application application, IServiceContainer serviceContainer, ITypeResolver typeResolver)
        {
            if (string.IsNullOrWhiteSpace(application.StateManagerType))
            {
                serviceContainer.RegisterService(typeof(IStateManager), () => new BucketStateManager(serviceContainer.GetService<IBucketExtension>()));
            }
            else
            {
                var stateManagerType = typeResolver.Resolve(application.StateManagerType);
                serviceContainer.RegisterService(typeof(IStateManager), () => serviceContainer.GetService(stateManagerType));
            }
        }


        private static void RegisterTunnelReceivers(Application application)
        {
            // Message
            var messageReceivers = new List<MessageApplicationReceiver>();
            if (application.MessageReceivers != null
                && application.MessageReceivers.Length > 0)
            {
                messageReceivers.AddRange(application.MessageReceivers);
            }
            messageReceivers.Add(
                new MessageApplicationReceiver
                {
                    Sender = $"(.+)@{TunnelExtension.TunnelAddress.Domain.Replace(".", "\\.")}\\/(.+)",
                    Type = nameof(TunnelMessageReceiver)
                });

            application.MessageReceivers = messageReceivers.ToArray();

            // Notification
            var notificationReceivers = new List<NotificationApplicationReceiver>();
            if (application.NotificationReceivers != null
                && application.NotificationReceivers.Length > 0)
            {
                notificationReceivers.AddRange(application.NotificationReceivers);
            }
            notificationReceivers.Add(
                new NotificationApplicationReceiver
                {
                    Sender = $"(.+)@{TunnelExtension.TunnelAddress.Domain.Replace(".", "\\.")}\\/(.+)",
                    Type = nameof(TunnelNotificationReceiver)
                });

            application.NotificationReceivers = notificationReceivers.ToArray();
        }


        private static async Task AddNotificationReceivers(
            Application application,
            IServiceContainer serviceContainer,
            ISender sender,
            IBlipChannelListener channelListener,
            ITypeResolver typeResolver,
            IStateManager stateManager,
            ISessionManager sessionManager)
        {
            if (application.NotificationReceivers != null && application.NotificationReceivers.Length > 0)
            {
                foreach (var applicationReceiver in application.NotificationReceivers)
                {
                    INotificationReceiver receiver;
                    if (applicationReceiver.Response?.MediaType != null)
                    {
                        var content = applicationReceiver.Response.ToDocument();
                        receiver =
                            new LambdaNotificationReceiver(
                                (notification, c) => sender.SendMessageAsync(content, notification.From, c));
                    }
                    else if (!string.IsNullOrWhiteSpace(applicationReceiver.ForwardTo))
                    {
                        var tunnelExtension = serviceContainer.GetService<ITunnelExtension>();
                        var destination = Identity.Parse(applicationReceiver.ForwardTo);
                        receiver =
                            new LambdaNotificationReceiver(
                                (notification, c) => tunnelExtension.ForwardNotificationAsync(notification, destination, c));
                    }
                    else
                    {
                        receiver = await CreateAsync<INotificationReceiver>(
                            applicationReceiver.Type, serviceContainer, applicationReceiver.Settings, typeResolver)
                            .ConfigureAwait(false);
                    }

                    if (applicationReceiver.OutState != null)
                    {
                        receiver = new SetStateNotificationReceiver(receiver, stateManager, applicationReceiver.OutState);
                    }

                    Func<Notification, Task<bool>> notificationPredicate = BuildPredicate<Notification>(applicationReceiver, stateManager, sessionManager);

                    if (applicationReceiver.EventType != null)
                    {
                        var currentNotificationPredicate = notificationPredicate;
                        notificationPredicate = async (n) => await currentNotificationPredicate(n) && n.Event.Equals(applicationReceiver.EventType);
                    }

                    channelListener.AddNotificationReceiver(receiver, notificationPredicate, applicationReceiver.Priority);
                }
            }
        }

        public static async Task AddMessageReceivers(
            Application application,
            IServiceContainer serviceContainer,
            ISender sender,
            IBlipChannelListener channelListener,
            ITypeResolver typeResolver,
            IStateManager stateManager,
            ISessionManager sessionManager)
        {
            if (application.MessageReceivers != null && application.MessageReceivers.Length > 0)
            {
                foreach (var applicationReceiver in application.MessageReceivers)
                {
                    IMessageReceiver receiver;
                    if (applicationReceiver.Response?.MediaType != null)
                    {
                        var content = applicationReceiver.Response.ToDocument();
                        receiver =
                            new LambdaMessageReceiver(
                                (message, c) => sender.SendMessageAsync(content, message.From, c));
                    }
                    else if (!string.IsNullOrWhiteSpace(applicationReceiver.ForwardTo))
                    {
                        var tunnelExtension = serviceContainer.GetService<ITunnelExtension>();
                        var destination = Identity.Parse(applicationReceiver.ForwardTo);
                        receiver =
                            new LambdaMessageReceiver(
                                (message, c) => tunnelExtension.ForwardMessageAsync(message, destination, c));
                    }
                    else
                    {
                        var receiverType = typeResolver.Resolve(applicationReceiver.Type);
                        receiver = await BuildByLifetimeAsync(
                            applicationReceiver.Lifetime ?? application.DefaultMessageReceiverLifetime,
                            receiverType,
                            applicationReceiver.Settings,
                            serviceContainer);
                    }

                    if (applicationReceiver.OutState != null)
                    {
                        receiver = new SetStateMessageReceiver(receiver, stateManager, applicationReceiver.OutState);
                    }

                    Func<Message, Task<bool>> messagePredicate = BuildPredicate<Message>(applicationReceiver, stateManager, sessionManager);

                    if (applicationReceiver.MediaType != null)
                    {
                        var currentMessagePredicate = messagePredicate;
                        var mediaTypeRegex = new Regex(applicationReceiver.MediaType, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        messagePredicate = async (m) => await currentMessagePredicate(m) && mediaTypeRegex.IsMatch(m.Type.ToString());
                    }

                    if (applicationReceiver.Content != null)
                    {
                        var currentMessagePredicate = messagePredicate;
                        var contentRegex = new Regex(applicationReceiver.Content, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        messagePredicate = async (m) => await currentMessagePredicate(m) && contentRegex.IsMatch(m.Content.ToString());
                    }

                    channelListener.AddMessageReceiver(receiver, messagePredicate, applicationReceiver.Priority);
                }
            }
        }

        private static async Task AddCommandReceivers(
            Application application,
            IServiceContainer serviceContainer,
            IBlipChannelListener channelListener,
            ITypeResolver typeResolver)
        {
            if (application.CommandReceivers == null || application.CommandReceivers.Length == 0)
            {
                return;
            }

            foreach (var commandReceiver in application.CommandReceivers)
            {
                var receiver = await CreateAsync<ICommandReceiver>(
                           commandReceiver.Type, serviceContainer, commandReceiver.Settings, typeResolver)
                           .ConfigureAwait(false);

                Func<Command, Task<bool>> predicate = c => Task.FromResult(true);

                if (commandReceiver.Method.HasValue)
                {
                    var currentPredicate = predicate;
                    predicate = async (c) => await currentPredicate(c) && c.Method == commandReceiver.Method.Value;
                }

                if (commandReceiver.Uri != null)
                {
                    var limeUriRegex = new Regex(commandReceiver.Uri, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var currentPredicate = predicate;
                    predicate = async (c) => await currentPredicate(c) && limeUriRegex.IsMatch(c.Uri.ToString());
                }

                if (commandReceiver.ResourceUri != null)
                {
                    var resourceUriRegex = new Regex(commandReceiver.ResourceUri, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var currentPredicate = predicate;
                    predicate = async (c) => await currentPredicate(c) && resourceUriRegex.IsMatch(c.GetResourceUri().ToString());
                }

                if (commandReceiver.MediaType != null)
                {
                    var currentPredicate = predicate;
                    var mediaTypeRegex = new Regex(commandReceiver.MediaType, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    predicate = async (c) => await currentPredicate(c) && mediaTypeRegex.IsMatch(c.Type.ToString());
                }

                channelListener.AddCommandReceiver(receiver, predicate, commandReceiver.Priority);
            }
        }

        private static Func<T, Task<bool>> BuildPredicate<T>(
            ApplicationReceiver applicationReceiver,
            IStateManager stateManager,
            ISessionManager sessionManager) where T : Envelope, new()
        {
            Func<T, Task<bool>> predicate = m => Task.FromResult(m != null);

            if (applicationReceiver.Sender != null)
            {
                var currentPredicate = predicate;
                var senderRegex = new Regex(applicationReceiver.Sender, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                predicate = async (m) => await currentPredicate(m) && senderRegex.IsMatch(m.From.ToString());
            }

            if (applicationReceiver.Destination != null)
            {
                var currentPredicate = predicate;
                var destinationRegex = new Regex(applicationReceiver.Destination, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                predicate = async (m) => await currentPredicate(m) && destinationRegex.IsMatch(m.To.ToString());
            }

            if (applicationReceiver.State != null)
            {
                var currentPredicate = predicate;
                predicate = async (m) => await currentPredicate(m)
                    && (await stateManager.GetStateAsync(m.From.ToIdentity(), CancellationToken.None)).Equals(applicationReceiver.State, StringComparison.OrdinalIgnoreCase);
            }

            if (applicationReceiver.Culture != null)
            {
                var currentPredicate = predicate;
                predicate = async (m) => await currentPredicate(m)
                    && (await sessionManager.GetCultureAsync(m.From, CancellationToken.None) ?? "").Equals(applicationReceiver.Culture, StringComparison.OrdinalIgnoreCase);
            }

            return predicate;
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

        private static Task<IMessageReceiver> BuildByLifetimeAsync(ReceiverLifetime lifetime, Type receiverType, IDictionary<string, object> settings, IServiceContainer serviceContainer)
        {
            switch (lifetime)
            {
                case ReceiverLifetime.Scoped:
                    var messageReceiverFactory = GetMessageReceiverFactory(serviceContainer);
                    return Task.FromResult<IMessageReceiver>(new ScopedMessageReceiverWrapper(messageReceiverFactory, receiverType, settings));

                case ReceiverLifetime.Singleton:
                default:
                    return CreateAsync<IMessageReceiver>(receiverType, serviceContainer, settings);
            }
        }

        private static IMessageReceiverFactory GetMessageReceiverFactory(IServiceContainer container)
        {
            IMessageReceiverFactory containerResolved = null;
            try
            {
                containerResolved = container.GetService<IMessageReceiverFactory>();
            }
            catch
            {
            }

            return containerResolved ?? new MessageReceiverFactory(container);
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

        private class SetStateMessageReceiver : SetStateEnvelopeReceiver<Message>, IMessageReceiver
        {
            public SetStateMessageReceiver(IEnvelopeReceiver<Message> receiver, IStateManager stateManager, string state)
                : base(receiver, stateManager, state)
            {
            }
        }

        private class SetStateNotificationReceiver : SetStateEnvelopeReceiver<Notification>, INotificationReceiver
        {
            public SetStateNotificationReceiver(IEnvelopeReceiver<Notification> receiver, IStateManager stateManager, string state)
                : base(receiver, stateManager, state)
            {
            }
        }

        private class SetStateEnvelopeReceiver<T> : IEnvelopeReceiver<T> where T : Envelope
        {
            private readonly IEnvelopeReceiver<T> _receiver;
            private readonly string _state;
            private readonly IStateManager _stateManager;

            protected SetStateEnvelopeReceiver(IEnvelopeReceiver<T> receiver, IStateManager stateManager, string state)
            {
                if (state == null) throw new ArgumentNullException(nameof(state));
                _receiver = receiver;
                _state = state;
                _stateManager = stateManager;
            }

            public async Task ReceiveAsync(T envelope, CancellationToken cancellationToken = new CancellationToken())
            {
                await _receiver.ReceiveAsync(envelope, cancellationToken).ConfigureAwait(false);
                await _stateManager.SetStateAsync(envelope.From.ToIdentity(), _state, cancellationToken);
            }
        }

        private class ScopedMessageReceiverWrapper : IMessageReceiver
        {
            private readonly IMessageReceiverFactory _messageReceiverFactory;
            private readonly Type _receiverType;
            private readonly IDictionary<string, object> _settings;

            public ScopedMessageReceiverWrapper(IMessageReceiverFactory messageReceiverFactory, Type receiverType, IDictionary<string, object> settings)
            {
                _messageReceiverFactory = messageReceiverFactory;
                _receiverType = receiverType;
                _settings = settings;
            }

            public async Task ReceiveAsync(Message message, CancellationToken cancellationToken = default(CancellationToken))
            {
                var receiver = await _messageReceiverFactory.CreateAsync(_receiverType, _settings)
                        .ConfigureAwait(false);

                try
                {
                    await receiver
                            .ReceiveAsync(message, cancellationToken)
                            .ConfigureAwait(false);
                }
                finally
                {
                    await _messageReceiverFactory
                            .ReleaseAsync(receiver)
                            .ConfigureAwait(false);
                }
            }
        }

        private class MessageReceiverFactory : IMessageReceiverFactory
        {
            private IServiceProvider _provider;

            public MessageReceiverFactory(IServiceProvider provider)
            {
                _provider = provider;
            }

            public Task<IMessageReceiver> CreateAsync(Type receiverType, IDictionary<string, object> settings)
            {
                return CreateAsync<IMessageReceiver>(receiverType, _provider, settings);
            }

            public Task ReleaseAsync(IMessageReceiver receiver)
            {
                return Task.CompletedTask;
            }
        }
    }

    public interface IMessageReceiverFactory
    {
        Task<IMessageReceiver> CreateAsync(Type receiverType, IDictionary<string, object> settings);
        Task ReleaseAsync(IMessageReceiver receiver);
    }
}
