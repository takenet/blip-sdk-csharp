using Lime.Messaging.Contents;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Models;
using Take.Blip.Client;
using Xunit;
using Action = Take.Blip.Builder.Models.Action;
using Input = Take.Blip.Builder.Models.Input;

namespace Take.Blip.Builder.UnitTests.Diagnostics
{
    public class FlowTraceTests : FlowManagerTestsBase
    {
        [Fact]
        public async Task FlowWithHttpTraceSettingsShouldCallHttpTraceProcessor()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            var messageType = "text/plain";
            var variableName = "variableName1";
            var variableValue = "OutputVariable value 1";
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(variableValue);

            var messageContent = "Hello {{variableName1}}!";
            var expectedMessageContent = $"Hello {variableValue}!";

            var traceUrl = "http://myserver.com/tracing";

            var flow = new Flow()
            {
                Id = Guid.NewGuid().ToString(),
                States = new[]
                {
                    new State
                    {
                        Id = "root",
                        Root = true,
                        Input = new Input(),
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "ping"
                            }
                        }
                    },
                    new State
                    {
                        Id = "ping",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", messageContent}
                                }
                            }
                        }
                    }
                },
                Configuration = new Dictionary<string, string>
                {
                    { "TraceMode", "All" },
                    { "TraceTargetType", "Http" },
                    { "TraceTarget", traceUrl }
                }
            };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(input, User, Application, flow, CancellationToken);

            // Assert
            await Task.Delay(100); // The trace is asynchronous

            await TraceProcessor.Received(1).ProcessTraceAsync(
                Arg.Is<TraceEvent>(e =>
                    e.Settings.TargetType == TraceTargetType.Http &&
                    e.Settings.Target == traceUrl &&
                    e.Trace.User == User.ToString() &&
                    e.Trace.Input == input &&
                    e.Trace.States.Count == 2 &&
                    e.Trace.States.ToArray()[0].Id == "root" &&
                    e.Trace.States.ToArray()[1].Id == "ping"),

                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithLimeTraceSettingsShouldCallLimeTraceProcessor()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            var messageType = "text/plain";
            var variableName = "variableName1";
            var variableValue = "OutputVariable value 1";
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(variableValue);

            var messageContent = "Hello {{variableName1}}!";
            var expectedMessageContent = $"Hello {variableValue}!";

            var traceIndentity = new Identity(Guid.NewGuid().ToString(), "msging.net");

            var flow = new Flow()
            {
                Id = Guid.NewGuid().ToString(),
                States = new[]
                {
                    new State
                    {
                        Id = "root",
                        Root = true,
                        Input = new Input(),
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "ping"
                            }
                        }
                    },
                    new State
                    {
                        Id = "ping",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", messageContent}
                                }
                            }
                        }
                    }
                },
                Configuration = new Dictionary<string, string>
                {
                    { "TraceMode", "All" },
                    { "TraceTargetType", "Lime" },
                    { "TraceTarget", traceIndentity }
                }
            };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(input, User, Application, flow, CancellationToken);

            // Assert
            await Task.Delay(100); // The trace is asynchronous

            await TraceProcessor.Received(1).ProcessTraceAsync(
                Arg.Is<TraceEvent>(e =>
                    e.Settings.TargetType == TraceTargetType.Lime &&
                    e.Settings.Target == traceIndentity &&
                    e.Trace.User == User.ToString() &&
                    e.Trace.Input == input &&
                    e.Trace.States.Count == 2 &&
                    e.Trace.States.ToArray()[0].Id == "root" &&
                    e.Trace.States.ToArray()[1].Id == "ping"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowTraceSettingsShouldCallLimeTraceProcessor()
        {
            // Arrange
            var traceIdentity = new Identity(Guid.NewGuid().ToString(), "msging.net");
            var mode = "All";
            var targetType = "Lime";
            EnvelopeReceiverContext<Message>.Create(new Message
            {
                Id = Guid.NewGuid().ToString(),
                Metadata = new Dictionary<string, string>
                {
                    { "builder.trace.mode", mode },
                    { "builder.trace.targetType", targetType },
                    { "builder.trace.target", traceIdentity },
                }
            });
            var input = new PlainText() { Text = "Ping!" };
            var messageType = "text/plain";
            var variableName = "variableName1";
            var variableValue = "OutputVariable value 1";
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(variableValue);

            var messageContent = "Hello {{variableName1}}!";
            var expectedMessageContent = $"Hello {variableValue}!";

            var flow = new Flow()
            {
                Id = Guid.NewGuid().ToString(),
                States = new[]
                {
                    new State
                    {
                        Id = "root",
                        Root = true,
                        Input = new Input(),
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "ping"
                            }
                        }
                    },
                    new State
                    {
                        Id = "ping",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", messageContent}
                                }
                            }
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(input, User, Application, flow, CancellationToken);

            // Assert
            await Task.Delay(100); // The trace is asynchronous

            await TraceProcessor.Received(1).ProcessTraceAsync(
                Arg.Is<TraceEvent>(e =>
                    e.Settings.Mode == TraceMode.All &&
                    e.Settings.TargetType == TraceTargetType.Lime &&
                    e.Settings.Target == traceIdentity &&
                    e.Trace.User == User.ToString() &&
                    e.Trace.Input == input &&
                    e.Trace.States.Count == 2 &&
                    e.Trace.States.ToArray()[0].Id == "root" &&
                    e.Trace.States.ToArray()[1].Id == "ping"),
                Arg.Any<CancellationToken>());
        }
    }
}