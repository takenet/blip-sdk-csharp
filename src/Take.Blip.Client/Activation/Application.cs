using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Take.Blip.Client.Session;

namespace Take.Blip.Client.Activation
{
    /// <summary>
    /// Defines the configuration type for the application.json file.
    /// </summary>
    public class Application : SettingsContainer
    {
        public static JsonSerializerSettings SerializerSettings { get; }

        static Application()
        {
            SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            SerializerSettings.Converters.Add(new StringEnumConverter
            {
                CamelCaseText = true,
                AllowIntegerValues = true
            });
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the instance name for the connection.
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// Gets or sets the domain.
        /// </summary>
        /// <value>
        /// The domain.
        /// </value>
        public string Domain { get; set; }
        
        /// <summary>
        /// Gets the application identity combining the <see cref="Instance"/> and <see cref="Domain"/>.
        /// </summary>
        [IgnoreDataMember]
        public Identity Identity => new Identity(Identifier, Domain ?? Constants.DEFAULT_DOMAIN);

        /// <summary>
        /// Gets the application node combining the <see cref="Instance"/> and <see cref="Domain"/> and <see cref="Instance"/>.
        /// </summary>
        [IgnoreDataMember]
        public Node Node => new Node(Identifier, Domain ?? Constants.DEFAULT_DOMAIN, Instance);
        
        /// <summary>
        /// Gets or sets the Uri scheme.
        /// </summary>
        /// <value>
        /// The scheme.
        /// </value>
        public string Scheme { get; set; }

        /// <summary>
        /// Gets or sets the name of the host.
        /// </summary>
        /// <value>
        /// The name of the host.
        /// </value>
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the port to be used.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets the access key.
        /// </summary>
        /// <value>
        /// The access key.
        /// </value>
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the send timeout, in milliseconds.
        /// </summary>
        /// <value>
        /// The send timeout.
        /// </value>
        public int SendTimeout { get; set; }

        /// <summary>
        /// Gets or sets the default lifetime for MessageReceivers.
        /// </summary>
        /// <value>
        /// The lifetime.
        /// </value>
        public ReceiverLifetime DefaultMessageReceiverLifetime { get; set; }

        /// <summary>
        /// Gets or sets the messages receivers.
        /// </summary>
        /// <value>
        /// The receivers.
        /// </value>
        public MessageApplicationReceiver[] MessageReceivers { get; set; }

        /// <summary>
        /// Gets or sets the notifications receivers.
        /// </summary>
        /// <value>
        /// The receivers.
        /// </value>
        public NotificationApplicationReceiver[] NotificationReceivers { get; set; }

        /// <summary>
        /// Gets or sets the command receivers
        /// </summary>
        public CommandApplicationReceiver[] CommandReceivers { get; set; }

        /// <summary>
        /// Gets or sets the type for the startup .NET type. It must implement <see cref="IStartable"/> or <see cref="IFactory{IStartable}"/>.
        /// The start is called before the start of the sender.
        /// The type constructor must be parameterless or receive only a <see cref="IServiceProvider"/> instance plus a <see cref="IDictionary{TKey,TValue}"/> settings instance.
        /// </summary>
        /// <value>
        /// The type of the startup.
        /// </value>
        public string StartupType { get; set; }

        /// <summary>
        /// Gets or sets a type to be used as a service provider for dependency injection. It must be an implementation of <see cref="IServiceProvider"/>.
        /// </summary>
        public string ServiceProviderType { get; set; }

        /// <summary>
        /// Gets or sets a type to be used as the state manager for message receivers. It must be an implementation of <see cref="IStateManager"/>.
        /// </summary>
        public string StateManagerType { get; set; }

        /// <summary>
        /// Gets or sets the session encryption mode to be used
        /// </summary>
        /// <value>
        /// The encryption mode.
        /// </value>
        public SessionEncryption? SessionEncryption { get; set; }

        /// <summary>
        /// Gets or sets the session compression mode to be used
        /// </summary>
        /// <value>
        /// The compression mode.
        /// </value>
        public SessionCompression? SessionCompression { get; set; }

        /// <summary>
        /// Identifies the version of the application.json schema. It is used to validate if a package is not outdated.
        /// </summary>
        /// <value>
        /// The schema application.json version
        /// </value>
        public int SchemaVersion { get; set; }

        /// <summary>
        /// Informs the routing rule used to connect
        /// </summary>
        public RoutingRule? RoutingRule { get; set; }

        /// <summary>
        /// Informs the throughput interval between envelopes
        /// </summary>
        public int Throughput { get; set; }

        public bool DisableNotify { get; set; }

        public int? ChannelCount { get; set; }

        public Event[] ReceiptEvents { get; set; }

        /// <summary>
        /// Define the presence status to be set when connected.
        /// </summary>
        public PresenceStatus? PresenceStatus { get; set; }
        
        /// <summary>
        /// Indicates if the tunnel receivers for automatically forwarding envelopes
        /// should be registered.
        /// </summary>
        public bool RegisterTunnelReceivers { get; set; }

        public int? EnvelopeBufferSize { get; set; }

        /// <summary>
        /// Creates an instance of <see cref="Application"/> from a JSON input.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static Application ParseFromJson(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            return JsonConvert.DeserializeObject<Application>(json, SerializerSettings);
        }

        /// <summary>
        /// Creates an instance of <see cref="Application" /> from a JSON file.
        /// </summary>
        /// <param name="filePath">The path.</param>
        /// <returns></returns>
        public static Application ParseFromJsonFile(string filePath) => ParseFromJson(File.ReadAllText(filePath));
    }
}
