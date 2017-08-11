using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Listeners;
using Lime.Protocol.Security;
using Lime.Protocol.Serialization.Newtonsoft;
using Lime.Protocol.Server;
using Lime.Protocol.Util;
using Lime.Transport.Tcp;
#pragma warning disable 4014

namespace Take.Blip.Client.UnitTests
{
    public sealed class DummyServer : IDisposable, IStartable, IStoppable
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
            _transportListener = new TcpTransportListener(ListenerUri, null, new JsonNetSerializer());
            Channels = new List<ServerChannel>();
        }

        public Uri ListenerUri { get; }

        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await ListenerSemaphore.WaitAsync(cancellationToken);

            await _transportListener.StartAsync();

            ProducerConsumer.CreateAsync(
                c => _transportListener.AcceptTransportAsync(c),
                async transport =>
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
                        },
                        (n, a) => new AuthenticationResult(null, n), _cts.Token);
                   
                    var channelListener = new ChannelListener(
                        m => TaskUtil.TrueCompletedTask,
                        n => TaskUtil.TrueCompletedTask,
                        async c =>
                        {
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
                    Channels.Add(serverChannel);

                    return true;
                },
                _cts.Token);
        }

        public ICollection<ServerChannel> Channels { get; }

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
