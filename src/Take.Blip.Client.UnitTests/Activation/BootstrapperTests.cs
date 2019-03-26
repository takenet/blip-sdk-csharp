using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Server;
using Shouldly;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Session;
using Xunit;

namespace Take.Blip.Client.UnitTests.Activation
{
    public class BootstrapperTests : TestsBase
    {
        public BootstrapperTests()
        {
            Server = new DummyServer();
            Server.StartAsync(CancellationToken).Wait();
        }

        public DummyServer Server { get; }

        public ITypeResolver TypeResolver { get; } =
            new TypeResolver(new AssemblyProvider(typeof(BootstrapperTests).GetTypeInfo().Assembly, typeof(BlipClient).GetTypeInfo().Assembly));

        [Fact]
        public void EnsureDefaultApplicationJsonValuesAreCorrect()
        {
            // Arrange
            var json = "{}";

            // Act
            var application = Application.ParseFromJson(json);

            // Assert
            application.SessionCompression.ShouldBe(null);
            application.SessionEncryption.ShouldBe(null);
        }

        [Fact]
        public async Task CreateWithNoCredentialAndNoReceiverShouldReturnInstance()
        {
            // Arrange
            var application = new Application()
            {
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
        }

        [Fact]
        public async Task CreateWithPassowrdAndNoReceiverShouldReturnInstance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                Password = "12345".ToBase64(),
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
        }

        [Fact]
        public async Task CreateWithAccessKeyAndNoReceiverShouldReturnInstance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
        }

        [Fact]
        public async Task CreateWithStartupTypeAndNoReceiverShouldReturnInstance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                StartupType = typeof(TestStartable).Name,
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            TestStartable._Started.ShouldBeTrue();
        }

        [Fact]
        public async Task CreateWithStartupTypeAndSettingsAndNoReceiverShouldReturnInstance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                StartupType = typeof(SettingsTestStartable).Name,
                Settings = new Dictionary<string, object>()
                {
                    { "setting1", "value1" },
                    { "setting2", 2 }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            SettingsTestStartable._Started.ShouldBeTrue();
            SettingsTestStartable.Settings.ShouldNotBeNull();
            SettingsTestStartable.Settings["setting1"].ShouldBe("value1");
            SettingsTestStartable.Settings["setting2"].ShouldBe(2);
            SettingsTestStartable.Sender.ShouldNotBeNull();
        }

        [Fact]
        public async Task CreateWithStartupFactoryTypeAndNoReceiverShouldReturnInstance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                StartupType = typeof(TestStartableFactory).AssemblyQualifiedName,
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            TestStartable._Started.ShouldBeTrue();
            TestStartableFactory.ServiceProvider.ShouldNotBeNull();
            TestStartableFactory.Settings.ShouldBeNull();
        }

        [Fact]
        public async Task CreateWithStartupFactoryTypeAndSetingsAndNoReceiverShouldReturnInstance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                StartupType = typeof(TestStartableFactory).AssemblyQualifiedName,
                Settings = new Dictionary<string, object>()
                {
                    { "setting1", "value1" },
                    { "setting2", 2 }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            TestStartable._Started.ShouldBeTrue();
            TestStartableFactory.ServiceProvider.ShouldNotBeNull();
            TestStartableFactory.Settings.ShouldNotBeNull();
            TestStartableFactory.Settings["setting1"].ShouldBe("value1");
            TestStartableFactory.Settings["setting2"].ShouldBe(2);
        }

        [Fact]
        public async Task CreateWithMessageReceiverTypeShouldReturnInstance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver()
                    {
                        Type = typeof(TestMessageReceiver).Name,
                        MediaType = "text/plain"
                    },
                    new MessageApplicationReceiver()
                    {
                        Type = typeof(TestMessageReceiver).Name,
                        MediaType = "application/json"
                    },
                    new MessageApplicationReceiver()
                    {
                        Type = typeof(TestMessageReceiver).AssemblyQualifiedName
                    }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            TestMessageReceiver.InstanceCount.ShouldBe(3);
        }

        [Fact]
        public async Task CreateWithMessageReceiverTypeAndScopedLifetimeShouldNotReturn_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver()
                    {
                        Type = typeof(TestMessageReceiver).Name,
                        MediaType = "text/plain",
                        Lifetime = ReceiverLifetime.Scoped
                    },
                    new MessageApplicationReceiver()
                    {
                        Type = typeof(TestMessageReceiver).Name,
                        MediaType = "application/json"
                    },
                    new MessageApplicationReceiver()
                    {
                        Type = typeof(TestMessageReceiver).AssemblyQualifiedName
                    }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            TestMessageReceiver.InstanceCount.ShouldBe(2);
        }

        [Fact]
        public async Task CreateWithRegisteringTunnelShouldAddReceiver()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                RegisterTunnelReceivers = true,
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver()
                    {
                        Type = typeof(TestMessageReceiver).Name,
                        MediaType = "text/plain"
                    }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            TestMessageReceiver.InstanceCount.ShouldBe(1);
        }

        [Fact]
        public async Task CreateWithMessageForwardToShouldAddReceiver()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                RegisterTunnelReceivers = true,
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver()
                    {
                        ForwardTo = "bot@msging.net"
                    }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
        }

        [Fact]
        public async Task CreateWithMessageReceiverTypeWithSettingsShouldReturnInstance()
        {
            // Arrange
            var application = new Application
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver
                    {
                        Type = typeof(TestMessageReceiver).Name,
                        MediaType = "text/plain",
                        Settings = new Dictionary<string, object>()
                        {
                            { "setting3", "value3" },
                            { "setting4", 4 },
                            { "setting5", 55 }
                        }
                    }
                },
                Settings = new Dictionary<string, object>
                {
                    { "setting1", "value1" },
                    { "setting2", 2 },
                    { "setting5", 5 }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            TestMessageReceiver.InstanceCount.ShouldBe(1);
            TestMessageReceiver.Settings.Count.ShouldBe(3);
            TestMessageReceiver.Settings["setting3"].ShouldBe("value3");
            TestMessageReceiver.Settings["setting4"].ShouldBe(4);
            TestMessageReceiver.Settings["setting5"].ShouldBe(55);
        }

        [Fact]
        public void CreateWithNotInheritingMessageReceiverShouldThrow()
        {
            // Arrange
            var application = new Application
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver
                    {
                        Type = typeof(InvalidReceiver).Name,
                    }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act & Assert
            Should.Throw<Exception>(async () =>
            {
                var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);
            });
        }

        [Fact]
        public void CreateWithSameNameClassMessageReceiverShouldThrow()
        {
            // Arrange
            var application = new Application
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver
                    {
                        Type = typeof(DuplicatedReceiver).Name,
                    }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act & Assert
            Should.Throw<Exception>(async () =>
            {
                var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);
            });
        }

        [Fact]
        public async Task CreateWithNotificationReceiverTypeShouldReturnInstance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                NotificationReceivers = new[]
                {
                    new NotificationApplicationReceiver()
                    {
                        Type = typeof(TestNotificationReceiver).AssemblyQualifiedName,
                        EventType = Event.Accepted
                    },
                    new NotificationApplicationReceiver()
                    {
                        Type = typeof(TestNotificationReceiver).AssemblyQualifiedName,
                        EventType = Event.Dispatched
                    },
                    new NotificationApplicationReceiver()
                    {
                        Type = typeof(TestNotificationReceiver).AssemblyQualifiedName
                    }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            TestNotificationReceiver.InstanceCount.ShouldBe(3);
        }

        [Fact]
        public async Task CreateWithNotificationForwardToShouldAddReceiver()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                RegisterTunnelReceivers = true,
                NotificationReceivers = new[]
                {
                    new NotificationApplicationReceiver()
                    {
                        ForwardTo = "bot@msging.net"
                    }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application);

            // Assert
            actual.ShouldNotBeNull();
        }

        [Fact]
        public async Task CreateWithCommandReceiverTypeShouldReturnInstance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                CommandReceivers = new[]
                {
                    new CommandApplicationReceiver()
                    {
                        Type = typeof(TestCommandReceiver).Name,
                        Method = CommandMethod.Get
                    },
                    new CommandApplicationReceiver()
                    {
                        Type = typeof(TestCommandReceiver).Name,
                        Method = CommandMethod.Set
                    },
                    new CommandApplicationReceiver()
                    {
                        Type = typeof(TestCommandReceiver).Name,
                        Method = CommandMethod.Subscribe
                    }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            TestCommandReceiver.InstanceCount.ShouldBe(3);
        }

        [Fact]
        public async Task CreateWithCommandReceiverTypeWithSettingsShouldReturnInstance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                CommandReceivers = new[]
                {
                    new CommandApplicationReceiver()
                    {
                        Type = typeof(TestCommandReceiver).Name,
                        Method = CommandMethod.Get,
                        ResourceUri = "/contacts"
                    },
                    new CommandApplicationReceiver()
                    {
                        Type = typeof(TestCommandReceiver).Name,
                        Method = CommandMethod.Set,
                        Uri = "lime://configuration/first"
                    },
                    new CommandApplicationReceiver()
                    {
                        Type = typeof(TestCommandReceiver).Name,
                        Method = CommandMethod.Subscribe,
                        ResourceUri = "lime://configuration/second"
                    }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            TestCommandReceiver.InstanceCount.ShouldBe(3);
        }

        [Fact]
        public async Task CreateWithCustomServiceProvider()
        {
            // Arrange
            var application = new Application
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver
                    {
                        Type = typeof(TestMessageReceiverWithCustomParameter).Name,
                        MediaType = "text/plain"
                    }
                },
                HostName = Server.ListenerUri.Host,
                ServiceProviderType = typeof(TestServiceProvider).Name
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            TestMessageReceiverWithCustomParameter.InstanceCount.ShouldBe(1);
            TestMessageReceiverWithCustomParameter.Sender.ShouldNotBeNull();
            TestMessageReceiverWithCustomParameter.Dependency.ShouldNotBeNull();
        }

        [Fact]
        public async Task CreateWithCustomServiceContainer()
        {
            // Arrange
            var application = new Application
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver
                    {
                        Type = nameof(SimpleMessageReceiver),
                        MediaType = "text/plain"
                    }
                },
                HostName = Server.ListenerUri.Host,
                ServiceProviderType = nameof(TestServiceContainer)
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            TestServiceContainer.CurrentInstance.ShouldNotBeNull();
            TestServiceContainer.CurrentInstance.Registrations.ShouldContainKeyAndValue(typeof(IServiceContainer), TestServiceContainer.CurrentInstance);
            TestServiceContainer.CurrentInstance.Registrations.ShouldContainKey(typeof(ISender));
            TestServiceContainer.CurrentInstance.Registrations.Keys.Count.ShouldBeGreaterThan(10);
        }

        [Fact]
        public async Task CreateWithCustomStateManager()
        {
            // Arrange
            var application = new Application
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver
                    {
                        Type = nameof(SimpleMessageReceiver),
                        MediaType = "text/plain"
                    }
                },
                HostName = Server.ListenerUri.Host,
                StateManagerType = nameof(TestStateManager),
                ServiceProviderType = nameof(TestServiceContainerWithStateManager)
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();
            TestStateManager.CurrentInstance.ShouldNotBeNull();
            var factoryRegistration = TestServiceContainer.CurrentInstance.Registrations[typeof(IStateManager)];
            var factory = factoryRegistration.ShouldBeOfType<Func<object>>();
            var instance = factory();
            instance.ShouldBeOfType<TestStateManager>();
        }

        [Fact]
        public async Task CreateWithCustomSettings()
        {
            // Arrange
            var application = new Application
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver
                    {
                        Type = typeof(TestMessageReceiverWithCustomSettings).Name,
                        MediaType = "text/plain",
                        SettingsType = typeof(TestMessageReceiverSettings).Name,
                        Settings = new Dictionary<string, object>
                        {
                            { "setting1", "value1" },
                            { "setting2", 22 }
                        }
                    }
                },
                HostName = Server.ListenerUri.Host,
                ServiceProviderType = typeof(TestServiceProvider).Name,
                SettingsType = typeof(TestApplicationSettings).Name,
                Settings = new Dictionary<string, object>
                {
                    { "setting2", 2 },
                    { "setting3", "3" }
                },
                StartupType = typeof(TestStartupWithCustomSettings).Name
            };

            // Act
            var actual = await Bootstrapper.StartAsync(CancellationToken, application, typeResolver: TypeResolver);

            // Assert
            actual.ShouldNotBeNull();

            TestMessageReceiverWithCustomSettings.InstanceCount.ShouldBe(1);
            TestMessageReceiverWithCustomSettings.DefaultSettings.Count.ShouldBe(2);
            TestMessageReceiverWithCustomSettings.DefaultSettings["setting1"].ShouldBe("value1");
            TestMessageReceiverWithCustomSettings.DefaultSettings["setting2"].ShouldBe(22);

            TestMessageReceiverWithCustomSettings.CustomSettings.Setting1.ShouldBe("value1");
            TestMessageReceiverWithCustomSettings.CustomSettings.Setting2.ShouldBe(22);

            TestStartupWithCustomSettings.InstanceCount.ShouldBe(1);
            TestStartupWithCustomSettings.DefaultSettings.Count.ShouldBe(2);
            TestStartupWithCustomSettings.DefaultSettings["setting2"].ShouldBe(2);
            TestStartupWithCustomSettings.DefaultSettings["setting3"].ShouldBe("3");

            TestStartupWithCustomSettings.CustomSettings.Setting2.ShouldBe(2);
            TestStartupWithCustomSettings.CustomSettings.Setting3.ShouldBe("3");

            TestMessageReceiverWithCustomSettings.TestApplicationSettings.ShouldBe(TestStartupWithCustomSettings.CustomSettings);
        }

        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Server.StopAsync(CancellationToken).Wait();
                Server.Dispose();
                TestMessageReceiver.InstanceCount = 0;
                TestNotificationReceiver.InstanceCount = 0;
                TestCommandReceiver.InstanceCount = 0;
            }

            base.Dispose(disposing);
        }
    }

    public class InvalidReceiver
    {
    }

    /// <summary>
    /// Another class with same name exists on folder Dummies
    /// </summary>
    public class DuplicatedReceiver
    {
    }

    public class TestStartupWithCustomSettings : IStartable
    {
        public static IDictionary<string, object> DefaultSettings { get; set; }
        public static TestApplicationSettings CustomSettings { get; set; }

        public static int InstanceCount;

        public TestStartupWithCustomSettings(IDictionary<string, object> defaultSettings, TestApplicationSettings customSettings)
        {
            DefaultSettings = defaultSettings;
            CustomSettings = customSettings;
            InstanceCount++;
        }

        public Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }
    }

    public class TestMessageReceiverWithCustomSettings : IMessageReceiver
    {
        public static TestMessageReceiverSettings CustomSettings { get; set; }
        public static TestApplicationSettings TestApplicationSettings { get; set; }
        public static IDictionary<string, object> DefaultSettings { get; set; }

        public static int InstanceCount;

        public TestMessageReceiverWithCustomSettings(TestMessageReceiverSettings customSettings, TestApplicationSettings testApplicationSettings, IDictionary<string, object> defaultSettings)
        {
            CustomSettings = customSettings;
            TestApplicationSettings = testApplicationSettings;
            DefaultSettings = defaultSettings;
            InstanceCount++;
        }

        public Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }
    }

    public class TestMessageReceiverSettings
    {
        public string Setting1 { get; set; }
        public int Setting2 { get; set; }
    }

    public class TestApplicationSettings
    {
        public int Setting2 { get; set; }
        public string Setting3 { get; set; }
    }

    public class TestMessageReceiverWithCustomParameter : IMessageReceiver
    {
        public static TestCustomType Dependency { get; private set; }
        public static ISender Sender { get; private set; }
        public static int InstanceCount;

        public TestMessageReceiverWithCustomParameter(TestCustomType dependency, ISender sender)
        {
            Dependency = dependency;
            Sender = sender;
            InstanceCount++;
        }

        public Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }
    }

    public class TestCustomType
    {
    }

    public class TestServiceProvider : IServiceProvider
    {
        public Type SingleInjectedType = typeof(TestCustomType);

        public object GetService(Type serviceType)
        {
            return serviceType == SingleInjectedType ? new TestCustomType() : null;
        }
    }

    public class TestServiceContainer : IServiceContainer
    {
        public static TestServiceContainer CurrentInstance;

        public readonly IDictionary<Type, object> Registrations;

        public TestServiceContainer()
        {
            Registrations = new Dictionary<Type, object>();
            CurrentInstance = this;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                if (Registrations.TryGetValue(serviceType, out var service))
                    return service;

                return Activator.CreateInstance(serviceType);
            }
            catch
            {
                return null;
            }
        }

        public void RegisterService(Type serviceType, object instance)
        {
            Registrations.Add(serviceType, instance);
        }

        public void RegisterService(Type serviceType, Func<object> instanceFactory)
        {
            Registrations.Add(serviceType, instanceFactory);
        }

        public void RegisterDecorator(Type serviceType, Func<object> instanceFactory)
        {
            Registrations.Remove(serviceType);
            Registrations.Add(serviceType, instanceFactory);
        }
    }

    public class TestStateManager : IStateManager
    {
        public static TestStateManager CurrentInstance;

        public TestStateManager()
        {
            CurrentInstance = this;
        }

        public TimeSpan StateTimeout { get; set; }

        public event EventHandler<StateEventArgs> StateChanged;

        public Task<string> GetStateAsync(Identity identity, CancellationToken cancellationToken)
        {
            return Task.FromResult(Constants.DEFAULT_STATE);
        }

        public Task ResetStateAsync(Identity identity, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SetStateAsync(Identity identity, string state, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class TestServiceContainerWithStateManager : TestServiceContainer
    {
        public TestServiceContainerWithStateManager()
        {
            Registrations.Add(typeof(TestStateManager), new TestStateManager());
        }
    }

    public class TestStartable : IStartable
    {
        public static bool _Started;

        public bool Started => _Started;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _Started = true;
            return Task.CompletedTask;
        }
    }

    public class SettingsTestStartable : IStartable
    {
        public static ISender Sender { get; private set; }

        public SettingsTestStartable(ISender sender, IDictionary<string, object> settings)
        {
            Sender = sender;
            Settings = settings;
        }

        public bool Started => _Started;

        public static bool _Started;

        public static IDictionary<string, object> Settings;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _Started = true;
            return Task.CompletedTask;
        }
    }

    public class TestStartableFactory : IFactory<IStartable>
    {
        public static IServiceProvider ServiceProvider;

        public static IDictionary<string, object> Settings;

        public Task<IStartable> CreateAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings)
        {
            ServiceProvider = serviceProvider;
            Settings = settings;
            return Task.FromResult<IStartable>(new TestStartable());
        }
    }

    public class SimpleMessageReceiver : IMessageReceiver
    {
        public Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }

    public class TestMessageReceiver : IMessageReceiver
    {
        public static int InstanceCount;
        public static IDictionary<string, object> Settings;

        public TestMessageReceiver(IDictionary<string, object> settings)
        {
            InstanceCount++;
            Settings = settings;
        }

        public Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class TestNotificationReceiver : INotificationReceiver
    {
        public static int InstanceCount;
        public static IDictionary<string, object> Settings;

        public TestNotificationReceiver(IDictionary<string, object> settings)
        {
            InstanceCount++;
            Settings = settings;
        }

        public Task ReceiveAsync(Notification envelope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class TestCommandReceiver : ICommandReceiver
    {
        public static int InstanceCount;
        public static IDictionary<string, object> Settings;

        public TestCommandReceiver(IDictionary<string, object> settings)
        {
            InstanceCount++;
            Settings = settings;
        }

        public Task ReceiveAsync(Command envelope, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}