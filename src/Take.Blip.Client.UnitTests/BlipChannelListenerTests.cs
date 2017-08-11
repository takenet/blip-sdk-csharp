using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lime.Messaging.Contents;
using Lime.Protocol;
using NSubstitute;
using Shouldly;
using Xunit;
#pragma warning disable 4014

namespace Take.Blip.Client.UnitTests
{
    public class BlipChannelListenerTests : TestsBase
    {
        public ISender Sender { get; } = Substitute.For<ISender>();

        public bool AutoNotify { get; set; } = true;

        public FakeEstablishedReceiverChannel EstablishedReceiverChannel { get; } = new FakeEstablishedReceiverChannel();


        private BlipChannelListener GetTarget()
        {
            return new BlipChannelListener(Sender, AutoNotify);
        }

        [Fact]
        public async Task StartingShouldStartConsumingTheReceiver()
        {
            // Arrange
            var target = GetTarget();
            await EstablishedReceiverChannel.MessageBuffer.SendAsync(Dummy.CreateMessage());
            await EstablishedReceiverChannel.NotificationBuffer.SendAsync(Dummy.CreateNotification());
            await EstablishedReceiverChannel.CommandBuffer.SendAsync(Dummy.CreateCommand());

            // Act
            target.Start(EstablishedReceiverChannel);

            // Assert
            await Task.Delay(250);
            EstablishedReceiverChannel.MessageBuffer.Count.ShouldBe(0);
            EstablishedReceiverChannel.NotificationBuffer.Count.ShouldBe(0);
            EstablishedReceiverChannel.CommandBuffer.Count.ShouldBe(0);
        }


        [Fact]
        public void StartingTwiceShouldThrowInvalidOperationException()
        {
            // Arrange
            var target = GetTarget();

            // Act
            target.Start(EstablishedReceiverChannel);

            // Assert
            Assert.Throws<InvalidOperationException>(() => target.Start(EstablishedReceiverChannel));
        }

        [Fact]
        public async Task RegisteringMessageReceiverWithoutPredicateShouldBeCalledWhenAnyMessageIsReceived()
        {
            // Arrange
            var target = GetTarget();
            var messageReceiver = Substitute.For<IMessageReceiver>();            
            var message = Dummy.CreateMessage();
            await EstablishedReceiverChannel.MessageBuffer.SendAsync(message);

            // Act
            target.AddMessageReceiver(messageReceiver);
            target.Start(EstablishedReceiverChannel);

            // Assert
            await Task.Delay(250);
            messageReceiver.Received(1).ReceiveAsync(message, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task RegisteringMessageReceiverWithTextTypePredicateShouldBeCalledWhenTextMessageIsReceived()
        {
            // Arrange
            var target = GetTarget();
            var messageReceiver = Substitute.For<IMessageReceiver>();            
            var chatStateMessage = Dummy.CreateMessage(Dummy.CreateChatState());
            var textMessage = Dummy.CreateMessage(Dummy.CreateTextContent());
            await EstablishedReceiverChannel.MessageBuffer.SendAsync(chatStateMessage);
            await EstablishedReceiverChannel.MessageBuffer.SendAsync(textMessage);

            // Act
            target.AddMessageReceiver(messageReceiver, m => m.Type.Equals(PlainText.MediaType).AsCompletedTask());
            target.Start(EstablishedReceiverChannel);

            // Assert
            await Task.Delay(250);
            messageReceiver.Received(1).ReceiveAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
            messageReceiver.Received(1).ReceiveAsync(textMessage, Arg.Any<CancellationToken>());
        }
    }
}