using Lime.Messaging.Contents;
using Lime.Protocol;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using NSubstitute;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Content;
using Take.Blip.Client.Extensions.Scheduler;
using Takenet.Iris.Messaging.Resources;
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
        private readonly ILogger Logger;
        private readonly IInputExpirationCount _inputExpirationCount;
        private readonly IConfiguration _configuration;
        private readonly IStateManager _stateManager;

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
            Logger = Substitute.For<ILogger>();
            _inputExpirationCount = Substitute.For<IInputExpirationCount>();
            _configuration = Substitute.For<IConfiguration>();
            _stateManager = Substitute.For<IStateManager>();
            InputHandler = new InputExpirationHandler(Scheduler, Logger, _inputExpirationCount, _stateManager, _configuration);
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
            var (_, returnMessage) = InputHandler.HandleMessage(Message);

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
            var flow = new Flow();

            // Act
            await InputHandler.OnFlowProcessedAsync(state, flow, Message, null, null, default(CancellationToken));

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
        public async Task WhenUserSendsMessage_WithoutInputExpiration()
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

            Scheduler
                .GetScheduledMessageAsync(Arg.Any<string>(), Arg.Any<Node>(), Arg.Any<CancellationToken>())
                .Returns(new Schedule
                {
                    Name = $"{UserIdentity}-inputexpirationtime",
                    Message = Message,
                    Status = ScheduleStatus.Scheduled,
                    When = DateTimeOffset.Now.Add(state.Input.Expiration ?? TimeSpan.Zero)
                });

            // Act
            await InputHandler.OnFlowPreProcessingAsync(state, Message, null, default(CancellationToken));

            // Assert
            await Scheduler
                .Received(1)
                .CancelScheduledMessageAsync(
                    Arg.Is<string>(s => s.Equals($"{UserIdentity}-inputexpirationtime")),
                    Arg.Any<Node>(),
                    Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));
            await _inputExpirationCount
              .Received(1)
              .TryRemoveAsync(Message);
        }

        [Fact]
        public async Task WhenUserSendsMessage_InputExpirationExceedingLimit()
        {
            // Arrange
            Message.Content = new InputExpiration() { Identity = UserIdentity, StateId = "Teste" };

            var state = new State
            {
                Id = "inputexpirationstateid",
                Input = new Builder.Models.Input()
                {
                    Expiration = TimeSpan.FromMinutes(1)
                }
            };

            var flow = new Flow();

            Message.Metadata = new Dictionary<string, string>
            {
                { InputExpirationHandler.STATE_ID, "inputexpirationstateid" },

            };

            _inputExpirationCount.IncrementAsync(Message).Returns(4);
            _configuration.MaximumInputExpirationLoop.Returns(3);
            
            // Act
            await InputHandler.OnFlowProcessedAsync(state, flow, Message, null, null, default(CancellationToken));

            // Assert
           await Scheduler
                 .DidNotReceive()
                 .ScheduleMessageAsync(
                     Arg.Any<Message>(),
                     Arg.Any<DateTimeOffset>(),
                     Arg.Any<Node>(),
                     Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));
            await _stateManager.Received(1).ResetUserState(null, default(CancellationToken));
            await _inputExpirationCount.Received(1).TryRemoveAsync(Message);
        }
        
        [Fact]
        public async Task WhenUserSendsMessage_InputExpirationWithLimit_Accept()
        {
            // Arrange
            Message.Content = new InputExpiration() { Identity = UserIdentity, StateId = "Teste" };

            var state = new State
            {
                Id = "inputexpirationstateid",
                Input = new Builder.Models.Input()
                {
                    Expiration = TimeSpan.FromMinutes(1)
                }
            };

            var flow = new Flow();

            Message.Metadata = new Dictionary<string, string>
            {
                { InputExpirationHandler.STATE_ID, "inputexpirationstateid" },

            };

            _inputExpirationCount.IncrementAsync(Message).Returns(3);
            _configuration.MaximumInputExpirationLoop.Returns(3);

            // Act
            await InputHandler.OnFlowProcessedAsync(state, flow, Message, null, null, default(CancellationToken));

            // Assert
            await Scheduler
                  .Received(1)
                  .ScheduleMessageAsync(
                      Arg.Is<Message>(c=> c.Content is InputExpiration),
                      Arg.Any<DateTimeOffset>(),
                      Arg.Any<Node>(),
                      Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));
            await _inputExpirationCount.Received(1).IncrementAsync(Message);
        }
    }
}

