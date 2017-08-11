using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Listeners;
using Lime.Protocol.Util;
using Shouldly;
using Xunit;

namespace Take.Blip.Client.UnitTests
{
    public class BlipClientBuilderTests : IDisposable
    {
        public BlipClientBuilderTests()
        {
            ChannelListener = new ChannelListener(
                m => TaskUtil.TrueCompletedTask,
                n => TaskUtil.TrueCompletedTask,
                c => TaskUtil.TrueCompletedTask);
            CancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            Server = new DummyServer();
        }

        public IChannelListener ChannelListener { get; }

        public CancellationTokenSource CancellationTokenSource { get; }

        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public DummyServer Server { get; }


        [Fact]
        public async Task BuildUsingAccessKeyShouldSucceed()
        {
            // Arrange
            var identifier = Dummy.CreateRandomString(10);
            var accessKey = Dummy.CreateRandomString(10);
            await Server.StartAsync(CancellationToken);

            // Act
            var actual = new BlipClientBuilder()
                .UsingHostName(Server.ListenerUri.Host)
                .UsingPort(Server.ListenerUri.Port)
                .UsingAccessKey(identifier, accessKey)
                .Build();

            await actual.StartAsync(ChannelListener, CancellationToken);

            // Assert
            Server.Channels.Count.ShouldBe(1);
            var serverChannel = Server.Channels.First();
            serverChannel.RemoteNode.Name.ShouldBe(identifier);
        }


        public void Dispose()
        {
            Server.StopAsync(CancellationToken).Wait();
            CancellationTokenSource.Dispose();
            Server.Dispose();
        }
    }
}
