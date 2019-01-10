using Lime.Protocol;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Take.Blip.Builder.Diagnostics;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Diagnostics
{
    public class TraceSettingsTests : CancellationTokenTestsBase
    {
        [Fact]
        public async Task CreateNewTraceSettingsFromMessageMetadaShouldSucceed()
        {
            var target = new Identity(Guid.NewGuid().ToString(), "msging.net");
            var mode = "All";
            var targetType = "Lime";
            var slowThreshold = "1000";
            var message = new Message
            {
                Metadata = new Dictionary<string, string>
                {
                    { "builder.trace.mode", mode },
                    { "builder.trace.targetType", targetType },
                    { "builder.trace.target", target },
                    { "builder.trace.slowThreshold", slowThreshold },
                }
            };

            var traceSettings = new TraceSettings(message.Metadata);
            traceSettings.Mode.ToString().ShouldBe(mode);
            traceSettings.TargetType.ToString().ShouldBe(targetType);
            traceSettings.Target.ShouldBe(target);
            traceSettings.SlowThreshold.ToString().ShouldBe(slowThreshold);
        }

        [Fact]
        public async Task CreateNewTraceSettingsFromMessageMetadaWithoutSlowThresholdShouldSucceed()
        {
            var target = new Identity(Guid.NewGuid().ToString(), "msging.net");
            var mode = "All";
            var targetType = "Lime";
            var message = new Message
            {
                Metadata = new Dictionary<string, string>
                {
                    { "builder.trace.mode", mode },
                    { "builder.trace.targetType", targetType },
                    { "builder.trace.target", target },
                }
            };

            var traceSettings = new TraceSettings(message.Metadata);
            traceSettings.Mode.ToString().ShouldBe(mode);
            traceSettings.TargetType.ToString().ShouldBe(targetType);
            traceSettings.Target.ShouldBe(target);
            traceSettings.SlowThreshold.ShouldBe(null);
        }

        [Fact]
        public async Task CreateNewTraceSettingsFromMessageMetadaWithoutModeShouldFail()
        {
            var target = new Identity(Guid.NewGuid().ToString(), "msging.net");
            var mode = "All";
            var targetType = "Lime";
            var message = new Message
            {
                Metadata = new Dictionary<string, string>
                {
                    { "builder.trace.targetType", targetType },
                    { "builder.trace.target", target },
                }
            };
            Should.Throw<ArgumentException>(() => new TraceSettings(message.Metadata));
        }

        [Fact]
        public async Task CreateNewTraceSettingsFromMessageMetadaWithoutTargetTypeShouldFail()
        {
            var target = new Identity(Guid.NewGuid().ToString(), "msging.net");
            var mode = "All";
            var targetType = "Lime";
            var message = new Message
            {
                Metadata = new Dictionary<string, string>
                {
                    { "builder.trace.mode", mode },
                    { "builder.trace.target", target },
                }
            };
            Should.Throw<ArgumentException>(() => new TraceSettings(message.Metadata));
        }

        [Fact]
        public async Task CreateNewTraceSettingsFromMessageMetadaWithoutTargetShouldFail()
        {
            var target = new Identity(Guid.NewGuid().ToString(), "msging.net");
            var mode = "All";
            var targetType = "Lime";
            var message = new Message
            {
                Metadata = new Dictionary<string, string>
                {
                    { "builder.trace.mode", mode },
                    { "builder.trace.targetType", targetType },
                }
            };
            Should.Throw<ArgumentException>(() => new TraceSettings(message.Metadata));
        }
    }
}