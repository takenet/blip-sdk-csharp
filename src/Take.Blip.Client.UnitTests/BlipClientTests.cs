using System;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Listeners;
using NSubstitute;
using Shouldly;
using Xunit;
#pragma warning disable 4014

namespace Take.Blip.Client.UnitTests
{
    public class BlipClientTests : TestsBase
    {
        public IOnDemandClientChannel OnDemandClientChannel { get; } = Substitute.For<IOnDemandClientChannel>();

        public IChannelListener ChannelListener { get; set; } = Substitute.For<IChannelListener>();

        public BlipClient GetTarget() => new BlipClient(OnDemandClientChannel);

        public async Task<BlipClient> GetAndStartTarget()
        {
            var target = GetTarget();
            await target.StartAsync(ChannelListener, CancellationToken);
            return target;
        }

        [Fact]
        public async Task StartShouldStartListenerAndEstablishOnDemandClientChannel()
        {
            // Arrange
            var target = GetTarget();

            // Act
            await target.StartAsync(ChannelListener, CancellationToken);

            // Assert
            ChannelListener.Received(1).Start(OnDemandClientChannel);
            OnDemandClientChannel.Received(1).EstablishAsync(CancellationToken);
        }

        [Fact]
        public async Task StartTwiceShouldThrowInvalidOperationException()
        {
            // Arrange
            var target = GetTarget();

            // Act                        
            await target.StartAsync(ChannelListener, CancellationToken);
            await target.StartAsync(ChannelListener, CancellationToken).ShouldThrowAsync<InvalidOperationException>();           
        }

        [Fact]
        public async Task StopShouldStopListenerAndFinishOnDemandClientChannel()
        {
            // Arrange
            var target = await GetAndStartTarget();

            // Act
            await target.StopAsync(CancellationToken);

            // Assert
            ChannelListener.Received(1).Stop();
            OnDemandClientChannel.Received(1).FinishAsync(CancellationToken);
        }

        [Fact]
        public async Task StopWithoutStartShouldThrowInvalidOperationException()
        {
            // Arrange
            var target = GetTarget();

            // Act
            await target.StopAsync(CancellationToken).ShouldThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task StopTwiceShouldThrowInvalidOperationException()
        {
            // Arrange
            var target = await GetAndStartTarget();

            // Act
            await target.StopAsync(CancellationToken);
            await target.StopAsync(CancellationToken).ShouldThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task ProcessComandShouldCallOnDemandClientChannel()
        {
            // Arrange
            var command = Dummy.CreateCommand();
            var target = await GetAndStartTarget();

            // Act
            await target.ProcessCommandAsync(command, CancellationToken);

            // Assert
            OnDemandClientChannel.Received(1).ProcessCommandAsync(command, CancellationToken);
        }

        [Fact]
        public async Task SendCommandShouldCallOnDemandClientChannel()
        {
            // Arrange
            var command = Dummy.CreateCommand();
            var target = await GetAndStartTarget();

            // Act
            await target.SendCommandAsync(command, CancellationToken);

            // Assert
            OnDemandClientChannel.Received(1).SendCommandAsync(command, CancellationToken);
        }

        [Fact]
        public async Task SendMessageShouldCallOnDemandClientChannel()
        {
            // Arrange
            var message = Dummy.CreateMessage(Dummy.CreateTextContent());
            var target = await GetAndStartTarget();

            // Act
            await target.SendMessageAsync(message, CancellationToken);

            // Assert
            OnDemandClientChannel.Received(1).SendMessageAsync(message, CancellationToken);
        }

        [Fact]
        public async Task SendNotificationShouldCallOnDemandClientChannel()
        {
            // Arrange
            var notification = Dummy.CreateNotification(Event.Received);
            var target = await GetAndStartTarget();

            // Act
            await target.SendNotificationAsync(notification, CancellationToken);

            // Assert
            OnDemandClientChannel.Received(1).SendNotificationAsync(notification, CancellationToken);
        }

        public void Dispose()
        {
            CancellationTokenSource.Dispose();
        }
    }
}
