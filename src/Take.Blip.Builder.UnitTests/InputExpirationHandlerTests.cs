using Lime.Messaging.Contents;
using Lime.Protocol;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Models;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Content;
using Take.Blip.Client.Extensions.Scheduler;
using Xunit;

namespace Take.Blip.Builder.UnitTests
{
    public class InputExpirationHandlerTests
    {
        public Identity UserIdentity { get; }
        public Identity ApplicationIdentity { get; }
        public Application Application { get; }
        public Message Message { get; }
        public InputExpirationHandler InputHandler { get; }

        private readonly ISchedulerExtension Scheduler;

        public InputExpirationHandlerTests()
        {
            UserIdentity = new Identity("user", "domain");
            ApplicationIdentity = new Identity("application", "domain");
            Application = new Application()
            {
                Identifier = ApplicationIdentity.Name,
                Domain = ApplicationIdentity.Domain
            };
            Message = new Message()
            {
                From = UserIdentity.ToNode(),
                To = ApplicationIdentity.ToNode()
            };

            Scheduler = Substitute.For<ISchedulerExtension>();

            InputHandler = new InputExpirationHandler(Scheduler);
        }

        [Fact]
        public async Task ValidateMessageDontChangeTraceMetadata()
        {
            // Arrange
            var traceIdentity = new Identity(Guid.NewGuid().ToString(), "msging.net");
            var mode = "All";
            var targetType = "Lime";

            Message.Metadata = new Dictionary<string, string>
            {
                { TraceSettings.BUILDER_TRACE_MODE, mode },
                { TraceSettings.BUILDER_TRACE_TARGET_TYPE, targetType },
                { TraceSettings.BUILDER_TRACE_TARGET, traceIdentity },
            };

            var messageContent = new InputExpiration() { Identity = UserIdentity, StateId = "Teste" };

            Message.Content = messageContent;

            // Act
            var returnMessage = InputHandler.ValidateMessage(Message);

            // Assert
            Assert.True(returnMessage.Metadata.ContainsKey(TraceSettings.BUILDER_TRACE_TARGET));
            Assert.True(returnMessage.Metadata.ContainsKey(TraceSettings.BUILDER_TRACE_TARGET_TYPE));
            Assert.True(returnMessage.Metadata.ContainsKey(TraceSettings.BUILDER_TRACE_MODE));
            Assert.True(returnMessage.Metadata.ContainsKey(InputExpirationHandler.STATE_ID));
            Assert.True(returnMessage.Metadata.ContainsKey(InputExpirationHandler.IDENTITY));
            Assert.False(Message.Metadata.ContainsKey(InputExpirationHandler.IDENTITY));
            Assert.False(Message.Metadata.ContainsKey(InputExpirationHandler.STATE_ID));
        }

        [Fact]
        public async Task WhenCreateInputExpirationMessageWithTraceMetadataThenTraceMetadataMustbePresent()
        {
            // Arrange
            var traceIdentity = new Identity(Guid.NewGuid().ToString(), "msging.net");
            var mode = "All";
            var targetType = "Lime";

            Message.Metadata = new Dictionary<string, string>
            {
                { TraceSettings.BUILDER_TRACE_MODE, mode },
                { TraceSettings.BUILDER_TRACE_TARGET_TYPE, targetType },
                { TraceSettings.BUILDER_TRACE_TARGET, traceIdentity },
            };

            var messageType = InputExpiration.MIME_TYPE;

            Message.Content = new PlainText() { Text = "Teste" };

            var state = new State
            {
                Id = "ping",
                Input = new Builder.Models.Input()
                {
                    Expiration = TimeSpan.FromMinutes(1)
                }
            };

            // Act
            await InputHandler.OnFlowProcessedAsync(state, Message, null, default(CancellationToken));

            // Assert
            Scheduler
                .Received(1)
                .ScheduleMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(ApplicationIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content is InputExpiration
                        && UserIdentity.Equals((m.Content as InputExpiration).Identity)
                        && state.Id.Equals((m.Content as InputExpiration).StateId)
                        && m.Metadata.ContainsKey(TraceSettings.BUILDER_TRACE_TARGET)
                        && m.Metadata.ContainsKey(TraceSettings.BUILDER_TRACE_TARGET_TYPE)
                        && m.Metadata.ContainsKey(TraceSettings.BUILDER_TRACE_MODE)
                        ),
                    Arg.Any<DateTimeOffset>(),
                    Arg.Any<Node>(),
                    Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));
        }

        [Fact]
        public async Task WhenUserSendMessageCancelInputExpirationScheduled()
        {
            // Arrange
            Message.Content = new PlainText() { Text = "Teste" };

            var state = new State
            {
                Id = "ping",
                Input = new Builder.Models.Input()
                {
                    Expiration = TimeSpan.FromMinutes(1)
                }
            };

            // Act
            await InputHandler.OnFlowPreProcessingAsync(state, Message, null, default(CancellationToken));

            // Assert
            Scheduler
                .Received(1)
                .CancelScheduledMessageAsync(
                    Arg.Is<string>(s => s.Equals($"{UserIdentity}-inputexpirationtime")),
                    Arg.Any<Node>(),
                    Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));
        }
    }
}

