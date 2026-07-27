using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Shouldly;
using Take.Blip.Builder.Actions.Trace;
using Take.Blip.Builder.Diagnostics;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class TraceActionTests : ActionTestsBase
    {
        private readonly Dictionary<string, object> _inputContext;
        private readonly InputTrace _inputTrace;

        public TraceActionTests()
        {
            _inputContext = new Dictionary<string, object>();
            Context.InputContext.Returns(_inputContext);

            _inputTrace = new InputTrace
            {
                Owner = OwnerIdentity,
                FlowId = "test-flow",
                User = UserIdentity.ToString(),
            };

            // Simulate what FlowManager does: store inputTrace in context
            _inputContext[ContextExtensions.CURRENT_INPUT_TRACE_KEY] = _inputTrace;
        }

        private static TraceAction GetTarget() => new TraceAction();

        [Fact]
        public async Task ExecuteWithRequiredFieldsShouldAddEntryToInputTrace()
        {
            // Arrange
            var settings = new JObject { ["name"] = "my-checkpoint" };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, settings, CancellationToken);

            // Assert
            _inputTrace.UserTraces.Count.ShouldBe(1);
            var entry = _inputTrace.UserTraces[0];
            entry.Name.ShouldBe("my-checkpoint");
            entry.Category.ShouldBeNull();
            entry.Value.ShouldBeNull();
            entry.Data.ShouldBeNull();
            entry.Timestamp.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task ExecuteWithAllFieldsShouldAddFullEntryToInputTrace()
        {
            // Arrange
            var settings = new JObject
            {
                ["name"] = "api-response",
                ["category"] = "http",
                ["value"] = "200",
                ["data"] = new JObject
                {
                    ["statusCode"] = "200",
                    ["url"] = "https://api.example.com/users",
                },
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, settings, CancellationToken);

            // Assert
            _inputTrace.UserTraces.Count.ShouldBe(1);
            var entry = _inputTrace.UserTraces[0];
            entry.Name.ShouldBe("api-response");
            entry.Category.ShouldBe("http");
            entry.Value.ShouldBe("200");
            entry.Data.ShouldNotBeNull();
            entry.Data["statusCode"].ShouldBe("200");
            entry.Data["url"].ShouldBe("https://api.example.com/users");
        }

        [Fact]
        public async Task ExecuteMultipleTimesShouldAppendAllEntriesToInputTrace()
        {
            // Arrange
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(
                Context,
                new JObject { ["name"] = "entry-1", ["category"] = "step" },
                CancellationToken
            );
            await target.ExecuteAsync(
                Context,
                new JObject { ["name"] = "entry-2", ["category"] = "step" },
                CancellationToken
            );
            await target.ExecuteAsync(
                Context,
                new JObject { ["name"] = "entry-3", ["category"] = "step" },
                CancellationToken
            );

            // Assert
            _inputTrace.UserTraces.Count.ShouldBe(3);
            _inputTrace.UserTraces[0].Name.ShouldBe("entry-1");
            _inputTrace.UserTraces[1].Name.ShouldBe("entry-2");
            _inputTrace.UserTraces[2].Name.ShouldBe("entry-3");
        }

        [Fact]
        public async Task ExecuteWhenTracingDisabledShouldCompleteWithoutError()
        {
            // Arrange: no inputTrace in context (tracing is disabled)
            _inputContext.Remove(ContextExtensions.CURRENT_INPUT_TRACE_KEY);
            var settings = new JObject { ["name"] = "should-be-ignored" };
            var target = GetTarget();

            // Act — should not throw
            await target.ExecuteAsync(Context, settings, CancellationToken);

            // Assert — nothing to assert; no exception means success
        }

        [Fact]
        public void ExecuteWithMissingNameShouldThrow()
        {
            // Arrange
            var settings = new JObject { ["category"] = "only-category" };
            var target = GetTarget();

            // Act & Assert — ActionBase.ExecuteAsync throws synchronously during Validate()
            Assert.Throws<ArgumentException>(
                (Action)(() => target.ExecuteAsync(Context, settings, CancellationToken))
            );
        }

        [Fact]
        public void ExecuteWithEmptyNameShouldThrow()
        {
            // Arrange
            var settings = new JObject { ["name"] = "" };
            var target = GetTarget();

            // Act & Assert — ActionBase.ExecuteAsync throws synchronously during Validate()
            Assert.Throws<ArgumentException>(
                (Action)(() => target.ExecuteAsync(Context, settings, CancellationToken))
            );
        }

        [Fact]
        public async Task ExecuteShouldRecordTimestampCloseToNow()
        {
            // Arrange
            var before = DateTimeOffset.UtcNow;
            var settings = new JObject { ["name"] = "timing-test" };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, settings, CancellationToken);

            // Assert
            var after = DateTimeOffset.UtcNow;
            var entry = _inputTrace.UserTraces[0];
            entry.Timestamp.ShouldBeGreaterThanOrEqualTo(before);
            entry.Timestamp.ShouldBeLessThanOrEqualTo(after);
        }

        [Fact]
        public void ActionTypeShouldBeTrace()
        {
            GetTarget().Type.ShouldBe("Trace");
        }
    }
}

