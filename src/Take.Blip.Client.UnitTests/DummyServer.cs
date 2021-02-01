using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging;
using Lime.Protocol;
using Lime.Protocol.Listeners;
using Lime.Protocol.Security;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using Lime.Protocol.Server;
using Lime.Protocol.Util;
using Lime.Transport.Tcp;
#pragma warning disable 4014

namespace Take.Blip.Client.UnitTests
{
    public sealed class DummyServer : IStartable, IStoppable, IDisposable
    {
        private readonly CancellationTokenSource _cts;
        private readonly TcpTransportListener _transportListener;

        private static readonly SemaphoreSlim ListenerSemaphore = new SemaphoreSlim(1, 1);

        public DummyServer()
            : this(new Uri("net.tcp://localhost:443"))
        {

        }

        public DummyServer(Uri listenerUri)
        {
            ListenerUri = listenerUri;
            _cts = new CancellationTokenSource();
            _transportListener = new TcpTransportListener(
                ListenerUri,
                null,
                new EnvelopeSerializer(new DocumentTypeResolver().WithMessagingDocuments()));
            Channels = new Queue<ServerChannel>();
            Authentications = new Queue<Authentication>();
            Messages = new Queue<Message>();
            Notifications = new Queue<Notification>();
            Commands = new Queue<Command>();
        }

        public Uri ListenerUri { get; }

        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await ListenerSemaphore.WaitAsync(cancellationToken);

            await _transportListener.StartAsync();

            ProducerConsumer.CreateAsync(
                c => _transportListener.AcceptTransportAsync(c),
                async (transport, _) =>
                {
                    await transport.OpenAsync(null, _cts.Token);

                    var serverChannel = new ServerChannel(
                        Guid.NewGuid().ToString(),
                        new Node("postmaster", "msging.net", "instance"),
                        transport,
                        TimeSpan.FromSeconds(60),
                        autoReplyPings: true);

                    await serverChannel.EstablishSessionAsync(
                        new[] { SessionCompression.None },
                        new[] { SessionEncryption.None },
                        new[]
                        {
                            AuthenticationScheme.Guest,
                            AuthenticationScheme.Key,
                            AuthenticationScheme.Plain,
                            AuthenticationScheme.Transport,
                            AuthenticationScheme.External
                        },
                        (n, a, _) =>
                        {
                            Authentications.Enqueue(a);
                            return new AuthenticationResult(DomainRole.RootAuthority, a).AsCompletedTask();
                        },
                        (n, s, c) =>
                        {
                            return n.AsCompletedTask();
                        }, _cts.Token);
                   
                    var channelListener = new ChannelListener(
                        m =>
                        {
                            Messages.Enqueue(m);
                            return TaskUtil.TrueCompletedTask;
                        },
                        n =>
                        {
                            Notifications.Enqueue(n);
                            return TaskUtil.TrueCompletedTask;
                        },
                        async c =>
                        {
                            Commands.Enqueue(c);
                            if (c.Status == CommandStatus.Pending)
                            {
                                await serverChannel.SendCommandAsync(
                                    new Command(c.Id)
                                    {
                                        Status = CommandStatus.Success,
                                        Method = c.Method
                                    },
                                    _cts.Token);
                            }
                            return true;
                        });

                    channelListener.Start(serverChannel);
                    Channels.Enqueue(serverChannel);

                    return true;
                },
                _cts.Token);
        }

        public Queue<ServerChannel> Channels { get; }

        public Queue<Authentication> Authentications { get; }

        public Queue<Message> Messages { get; }

        public Queue<Notification> Notifications { get; }

        public Queue<Command> Commands { get; }

        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _cts?.Cancel();
            await (_transportListener?.StopAsync() ?? Task.CompletedTask);

            ListenerSemaphore.Release();
        }

        public void Dispose()
        {
            _cts.Dispose();
            _transportListener?.DisposeIfDisposable();
        }
    }
}
