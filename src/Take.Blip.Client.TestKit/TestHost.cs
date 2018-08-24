using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lime.Messaging;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using Take.Blip.Client.Activation;

namespace Take.Blip.Client.TestKit
{
    public class TestHost
    {
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        private readonly TimeSpan _messageWaitTimeout;
        private readonly TimeSpan _notificationWaitTimeout;
        private readonly Assembly _assembly;
        private readonly ITypeResolver _typeResolver;

        private InternalOnDemandClientChannel _onDemandClientChannel;        
        private IBlipClient _client;
        private IBlipChannelListener _blipChannelListener;

        /// <summary>
        /// In-memory host for a Blip SDK chatbot implementation
        /// </summary>
        /// <param name="assembly">The assembly for the full chatbot implementation</param>
        /// <param name="messageWaitTimeout"></param>
        /// <param name="notificationWaitTimeout"></param>
        /// <param name="typeResolver"></param>
        public TestHost(Assembly assembly, TimeSpan? messageWaitTimeout = null, TimeSpan? notificationWaitTimeout = null, ITypeResolver typeResolver = null)
        {
            _assembly = assembly;
            _messageWaitTimeout = messageWaitTimeout ?? DefaultTimeout;
            _notificationWaitTimeout = notificationWaitTimeout ?? DefaultTimeout;
            _typeResolver = typeResolver ?? new TypeResolver(new AssemblyProvider(typeof(BlipClient).Assembly));
        }

        public async Task<IServiceContainer> StartAsync(Action<IServiceContainer> serviceOverrides = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var applicationFileName = Bootstrapper.DefaultApplicationFileName;
            var application = Application.ParseFromJsonFile(Path.Combine(GetAssemblyRoot(), applicationFileName));
            var typeResolver = new AggregateTypeResolver(new TypeResolver(new AssemblyProvider(_assembly)), _typeResolver);

            var localServiceProvider = BuildServiceContainer(application, typeResolver);
            localServiceProvider.RegisterService(typeof(IServiceProvider), localServiceProvider);
            localServiceProvider.RegisterService(typeof(IServiceContainer), localServiceProvider);
            localServiceProvider.RegisterService(typeof(Application), application);

            Bootstrapper.RegisterSettingsContainer(application, localServiceProvider, typeResolver);

            var serializer = new EnvelopeSerializer(new DocumentTypeResolver().WithMessagingDocuments());
            _onDemandClientChannel = new InternalOnDemandClientChannel(serializer, application);
            _client = await Bootstrapper.BuildClientAsync(
                application,
                () => new BlipClient(new InternalOnDemandClientChannel(serializer, application)),
                localServiceProvider,
                typeResolver,
                cancellationToken,
                serviceOverrides);

            // The listener should be active?
            _blipChannelListener = new BlipChannelListener(_client, !application.DisableNotify);

            await _client.StartAsync(_blipChannelListener, cancellationToken).ConfigureAwait(false);
            await Bootstrapper.BuildStartupAsync(application, localServiceProvider, typeResolver);
            Identity = Identity.Parse($"{application.Identifier}@{application.Domain ?? "msging.net"}");
            return localServiceProvider;
        }

        public Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _client?.StopAsync(cancellationToken);
        }

        /// <summary>
        /// Helper to indicate successfull initalization
        /// </summary>
        public bool IsListening => _client != null;

        public Identity Identity { get; private set; }

        /// <summary>
        /// Deliver a message to be processed by the chatbot
        /// </summary>
        public Task DeliverIncomingMessageAsync(Message message)
            => _onDemandClientChannel.IncomingMessages.SendAsync(message);

        /// <summary>
        /// Retrieve next chatbot generated message, using current message wait timeout
        /// (default: 1s)
        /// </summary>
        public Task<Message> RetrieveOutgoingMessageAsync()
            => RetrieveOutgoingMessageAsync(_messageWaitTimeout);

        /// <summary>
        /// Retrieve next bot generated message, using specified timeout
        /// </summary>
        public Task<Message> RetrieveOutgoingMessageAsync(TimeSpan timeout)
        {
            return _onDemandClientChannel.OutgoingMessages.ReceiveAsync(timeout);
        }

        /// <summary>
        /// Retrieve next chatbot generated notification, using current notification wait timeout
        /// (default: 1s)
        /// </summary>
        public Task<Notification> RetrieveOutgoingNotificationAsync()
            => RetrieveOutgoingNotificationAsync(_notificationWaitTimeout);

        /// <summary>
        /// Retrieve next bot generated notification, using specified timeout
        /// </summary>
        public Task<Notification> RetrieveOutgoingNotificationAsync(TimeSpan timeout)
        {
            return _onDemandClientChannel.OutgoingNotifications.ReceiveAsync(timeout);
        }


        private IServiceContainer BuildServiceContainer(Application application, ITypeResolver typeResolver)
        {
            var serviceProviderType = typeResolver.Resolve(application.ServiceProviderType);
            var serviceProvider = (IServiceProvider)Activator.CreateInstance(serviceProviderType);

            return new OverridableServiceContainer(serviceProvider);
        }

        private string GetAssemblyRoot()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
