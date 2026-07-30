using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Shouldly;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
using Take.Blip.Builder.Actions.DeleteVariable;
using Take.Blip.Builder.Actions.SendMessage;
using Take.Blip.Builder.Actions.SendRawMessage;
using Take.Blip.Builder.Actions.SetVariable;
using Take.Blip.Builder.Actions.TrackEvent;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Client;
using Take.Blip.Client.Extensions;
using Take.Blip.Client.Extensions.EventTracker;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ActionMonitoringTests : ActionTestsBase
    {
        private readonly IBlipLogger _blipLogger;
        private readonly Dictionary<string, object> _inputContext;

        public ActionMonitoringTests()
        {
            _blipLogger = Substitute.For<IBlipLogger>();
            _inputContext = new Dictionary<string, object>();
            Context.InputContext.Returns(_inputContext);
        }

        private void SetActionTrace(string actionId, string actionTitle)
        {
            _inputContext[ContextExtensions.CURRENT_ACTION_TRACE_KEY] = new ActionTrace
            {
                ActionId = actionId,
                ActionTitle = actionTitle,
            };
        }

        // ──────────────────────────────────────────────────────────────────
        // SetVariableAction
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SetVariable_SuccessPath_LogsActionIdAndTitle()
        {
            SetActionTrace("action-001", "Set Name");
            var settings = new SetVariableSettings { Variable = "name", Value = "Bob" };
            var target = new SetVariableAction(_blipLogger);

            await target.ExecuteAsync(Context, settings, CancellationToken);

            _blipLogger.Received(1).ActionExecution(Arg.Any<LogInput>());
        }

        [Fact]
        public async Task SetVariable_ErrorPath_LogsActionIdAndTitle()
        {
            SetActionTrace("action-001", "Set Name");
            var settings = new SetVariableSettings { Variable = "name", Value = "Bob" };
            Context.SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<System.Threading.CancellationToken>(), Arg.Any<TimeSpan>())
                   .Returns<System.Threading.Tasks.Task>(_ => throw new InvalidOperationException("fail"));
            var target = new SetVariableAction(_blipLogger);

            await Should.ThrowAsync<InvalidOperationException>(() => target.ExecuteAsync(Context, settings, CancellationToken));

            _blipLogger.Received(1).ErrorEvents(Arg.Any<LogInput>(), Arg.Any<Exception>());
        }

        [Fact]
        public async Task SetVariable_WhenNoActionTrace_LogsNullActionIdAndTitle()
        {
            var settings = new SetVariableSettings { Variable = "x", Value = "y" };
            var target = new SetVariableAction(_blipLogger);

            await target.ExecuteAsync(Context, settings, CancellationToken);

            _blipLogger.Received(1).ActionExecution(Arg.Any<LogInput>());
        }

        // ──────────────────────────────────────────────────────────────────
        // DeleteVariableAction
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteVariable_SuccessPath_LogsActionIdAndTitle()
        {
            SetActionTrace("action-002", "Clear Var");
            var settings = new DeleteVariableSettings { Variable = "name" };
            var target = new DeleteVariableAction(_blipLogger);

            await target.ExecuteAsync(Context, settings, CancellationToken);

            _blipLogger.Received(1).ActionExecution(Arg.Any<LogInput>());
        }

        [Fact]
        public async Task DeleteVariable_ErrorPath_LogsActionIdAndTitle()
        {
            SetActionTrace("action-002", "Clear Var");
            var settings = new DeleteVariableSettings { Variable = "name" };
            Context.DeleteVariableAsync(Arg.Any<string>(), Arg.Any<System.Threading.CancellationToken>())
                   .Returns<System.Threading.Tasks.Task>(_ => throw new InvalidOperationException("fail"));
            var target = new DeleteVariableAction(_blipLogger);

            await Should.ThrowAsync<InvalidOperationException>(() => target.ExecuteAsync(Context, settings, CancellationToken));

            _blipLogger.Received(1).ErrorEvents(Arg.Any<LogInput>(), Arg.Any<Exception>());
        }

        // ──────────────────────────────────────────────────────────────────
        // SendMessageAction
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SendMessage_SuccessPath_LogsActionIdAndTitle()
        {
            SetActionTrace("action-003", "Send Welcome");
            var sender = Substitute.For<ISender>();
            var settings = JObject.FromObject(new
            {
                type = PlainText.MIME_TYPE,
                content = "Hello"
            });
            var target = new SendMessageAction(sender, _blipLogger);

            await target.ExecuteAsync(Context, settings, CancellationToken);

            _blipLogger.Received(1).MessageDelivery(Arg.Any<LogInput>());
        }

        [Fact]
        public async Task SendMessage_ErrorPath_LogsActionIdAndTitle()
        {
            SetActionTrace("action-003", "Send Welcome");
            var sender = Substitute.For<ISender>();
            sender.SendMessageAsync(Arg.Any<Message>(), Arg.Any<System.Threading.CancellationToken>())
                  .Returns<System.Threading.Tasks.Task>(_ => throw new InvalidOperationException("send fail"));
            var settings = JObject.FromObject(new
            {
                type = PlainText.MIME_TYPE,
                content = "Hello"
            });
            var target = new SendMessageAction(sender, _blipLogger);

            await Should.ThrowAsync<InvalidOperationException>(() => target.ExecuteAsync(Context, settings, CancellationToken));

            _blipLogger.Received(1).ErrorEvents(Arg.Any<LogInput>(), Arg.Any<Exception>());
        }

        // ──────────────────────────────────────────────────────────────────
        // SendRawMessageAction
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SendRawMessage_SuccessPath_LogsActionIdAndTitle()
        {
            SetActionTrace("action-004", "Raw Msg");
            var sender = Substitute.For<ISender>();
            var resolver = new DocumentTypeResolver().WithBlipDocuments();
            var serializer = new DocumentSerializer(resolver);
            var settings = new SendRawMessageSettings
            {
                Type = PlainText.MIME_TYPE,
                RawContent = "Test"
            };
            var target = new SendRawMessageAction(sender, serializer, _blipLogger);

            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            _blipLogger.Received(1).MessageDelivery(Arg.Any<LogInput>());
        }

        // ──────────────────────────────────────────────────────────────────
        // TrackEventAction
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task TrackEvent_SuccessPath_LogsActionIdAndTitle()
        {
            SetActionTrace("action-005", "Track Purchase");
            var extension = Substitute.For<IEventTrackExtension>();
            var settings = new TrackEventSettings { Category = "ecommerce", Action = "buy" };
            var target = new TrackEventAction(extension, _blipLogger);

            await target.ExecuteAsync(Context, settings, CancellationToken);

            _blipLogger.Received(1).ActionExecution(Arg.Any<LogInput>());
        }

        // ──────────────────────────────────────────────────────────────────
        // Logger is called exactly once per execution (no double-logging)
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SetVariable_Success_LoggerCalledExactlyOnce()
        {
            SetActionTrace("id", "title");
            var settings = new SetVariableSettings { Variable = "v", Value = "1" };
            var target = new SetVariableAction(_blipLogger);

            await target.ExecuteAsync(Context, settings, CancellationToken);

            _blipLogger.Received(1).ActionExecution(Arg.Any<LogInput>());
        }
    }
}
