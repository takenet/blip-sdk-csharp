using System;
using Blip.Ai.Bot.Monitoring.Logging.Interface;
using Blip.Ai.Bot.Monitoring.Logging.Models;
using NSubstitute;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Monitoring;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Monitoring
{
    public class MonitoringDetailedFilterBlipLoggerDecoratorTests
    {
        [Fact]
        public void ErrorEvents_ShouldAlwaysForward_WhenTitleIsFiltered_AndDetailedDisabled()
        {
            var innerLogger = Substitute.For<IBlipLogger>();
            var configuration = Substitute.For<IConfiguration>();
            configuration.IsMonitoringDetailedEnabled.Returns(false);

            var sut = new MonitoringDetailedFilterBlipLoggerDecorator(innerLogger, configuration);
            var logInput = CreateLogInput("TrackEvent");
            var exception = new Exception("test");

            sut.ErrorEvents(logInput, exception);

            innerLogger.Received(1).ErrorEvents(logInput, exception);
        }

        [Fact]
        public void ConversationalFlow_ShouldForward_WhenDetailedEnabled_AndTitleIsFiltered()
        {
            var innerLogger = Substitute.For<IBlipLogger>();
            var configuration = Substitute.For<IConfiguration>();
            configuration.IsMonitoringDetailedEnabled.Returns(true);

            var sut = new MonitoringDetailedFilterBlipLoggerDecorator(innerLogger, configuration);
            var logInput = CreateLogInput("TrackEvent");

            sut.ConversationalFlow(logInput);

            innerLogger.Received(1).ConversationalFlow(logInput);
        }

        [Fact]
        public void ConversationalFlow_ShouldSuppress_WhenDetailedDisabled_AndTitleIsFiltered()
        {
            var innerLogger = Substitute.For<IBlipLogger>();
            var configuration = Substitute.For<IConfiguration>();
            configuration.IsMonitoringDetailedEnabled.Returns(false);

            var sut = new MonitoringDetailedFilterBlipLoggerDecorator(innerLogger, configuration);
            var logInput = CreateLogInput("TrackEvent");

            sut.ConversationalFlow(logInput);

            innerLogger.DidNotReceive().ConversationalFlow(Arg.Any<LogInput>());
        }

        [Fact]
        public void ConversationalFlow_ShouldForward_WhenDetailedDisabled_AndTitleIsNotFiltered()
        {
            var innerLogger = Substitute.For<IBlipLogger>();
            var configuration = Substitute.For<IConfiguration>();
            configuration.IsMonitoringDetailedEnabled.Returns(false);

            var sut = new MonitoringDetailedFilterBlipLoggerDecorator(innerLogger, configuration);
            var logInput = CreateLogInput("AgentHandoff");

            sut.ConversationalFlow(logInput);

            innerLogger.Received(1).ConversationalFlow(logInput);
        }

        private static LogInput CreateLogInput(string title) =>
            new LogInput
            {
                Title = title,
                FlowId = "flow-id",
                IdMessage = "message-id",
                From = "from@msging.net",
                OriginalFrom = "from@msging.net",
                To = "to@msging.net",
                OriginalTo = "to@msging.net",
                Operation = "operation",
                EventType = "event-type",
                StateId = "state-id",
            };
    }
}
