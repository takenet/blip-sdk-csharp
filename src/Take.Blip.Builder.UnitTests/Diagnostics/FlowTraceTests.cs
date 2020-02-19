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
            Message.Content = input;
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
                        Input = new Input(),
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            await Task.Delay(100); // The trace is asynchronous

            await TraceProcessor.Received(1).ProcessTraceAsync(
                Arg.Is<TraceEvent>(e =>
                    e.Settings.TargetType == TraceTargetType.Http &&
                    e.Settings.Target == traceUrl &&
                    e.Trace.User == UserIdentity.ToString() &&
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
            Message.Content = input;
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
                        Input = new Input(),
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            await Task.Delay(100); // The trace is asynchronous

            await TraceProcessor.Received(1).ProcessTraceAsync(
                Arg.Is<TraceEvent>(e =>
                    e.Settings.TargetType == TraceTargetType.Lime &&
                    e.Settings.Target == traceIndentity &&
                    e.Trace.User == UserIdentity.ToString() &&
                    e.Trace.Input == input &&
                    e.Trace.States.Count == 2 &&
                    e.Trace.States.ToArray()[0].Id == "root" &&
                    e.Trace.States.ToArray()[1].Id == "ping"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithSeveralActionsShouldProduceTwoTracesWithInputOnSecondState()
        {
            // Arrange
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
                        },
                        OutputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", messageContent}
                                }
                            },
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", messageContent}
                                }
                            },
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
                    },
                    new State
                    {
                        Id = "ping",
                        Input = new Input(),
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
                        },
                        OutputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", messageContent}
                                }
                            },
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
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            await Task.Delay(100); // The trace is asynchronous

            await TraceProcessor.Received(1).ProcessTraceAsync(
                Arg.Is<TraceEvent>(e =>
                    e.Settings.TargetType == TraceTargetType.Lime &&
                    e.Settings.Target == traceIndentity &&
                    e.Trace.User == UserIdentity.ToString() &&
                    e.Trace.Input == input &&
                    e.Trace.States.Count == 2 &&
                    e.Trace.States.ToArray()[0].Id == "root" &&
                    e.Trace.States.ToArray()[0].InputActions.Count == 0 &&
                    e.Trace.States.ToArray()[0].OutputActions.Count == 3 &&
                    e.Trace.States.ToArray()[1].Id == "ping" &&
                    e.Trace.States.ToArray()[1].InputActions.Count == 1 &&
                    e.Trace.States.ToArray()[1].OutputActions.Count == 0),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithSeveralActionsShouldProcuceThreeTracesWithoutInputOnSecondState()
        {
            // Arrange
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
                        },
                        OutputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", messageContent}
                                }
                            },
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", messageContent}
                                }
                            },
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
                        },
                        OutputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", messageContent}
                                }
                            },
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
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            await Task.Delay(100); // The trace is asynchronous

            await TraceProcessor.Received(1).ProcessTraceAsync(
                Arg.Is<TraceEvent>(e =>
                    e.Settings.TargetType == TraceTargetType.Lime &&
                    e.Settings.Target == traceIndentity &&
                    e.Trace.User == UserIdentity.ToString() &&
                    e.Trace.Input == input &&
                    e.Trace.States.Count == 2 &&
                    e.Trace.States.ToArray()[0].Id == "root" &&
                    e.Trace.States.ToArray()[0].InputActions.Count == 0 &&
                    e.Trace.States.ToArray()[0].OutputActions.Count == 3 &&
                    e.Trace.States.ToArray()[1].Id == "ping" &&
                    e.Trace.States.ToArray()[1].InputActions.Count == 1 &&
                    e.Trace.States.ToArray()[1].OutputActions.Count == 2),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowTraceSettingsShouldCallLimeTraceProcessor()
        {
            // Arrange
            var traceIdentity = new Identity(Guid.NewGuid().ToString(), "msging.net");
            var mode = "All";
            var targetType = "Lime";

            Message.Metadata = new Dictionary<string, string>
            {
                { "builder.trace.mode", mode },
                { "builder.trace.targetType", targetType },
                { "builder.trace.target", traceIdentity },
            };
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
                        Input = new Input(),
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
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            await Task.Delay(100); // The trace is asynchronous

            await TraceProcessor.Received(1).ProcessTraceAsync(
                Arg.Is<TraceEvent>(e =>
                    e.Settings.Mode == TraceMode.All &&
                    e.Settings.TargetType == TraceTargetType.Lime &&
                    e.Settings.Target == traceIdentity &&
                    e.Trace.User == UserIdentity.ToString() &&
                    e.Trace.Input == input &&
                    e.Trace.States.Count == 2 &&
                    e.Trace.States.ToArray()[0].Id == "root" &&
                    e.Trace.States.ToArray()[1].Id == "ping"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ThreeBlocksWithoutInputShouldResultOnThreeTraces()
        {
            // Arrange
            var traceIdentity = new Identity(Guid.NewGuid().ToString(), "msging.net");
            var mode = "All";
            var targetType = "Lime";

            Message.Metadata = new Dictionary<string, string>
            {
                { "builder.trace.mode", mode },
                { "builder.trace.targetType", targetType },
                { "builder.trace.target", traceIdentity },
            };
            var messageType = "text/plain";
            var pingMessageContent = "Ping!!";
            var pongMessageContent = "Pong!!";

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
                        Input = new Input{
                            Bypass = true
                        },
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", pingMessageContent}
                                }
                            }
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "pong"
                            }
                        }
                    },
                    new State
                    {
                        Id = "pong",
                        Input = new Input(),
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", pongMessageContent}
                                }
                            }
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            await Task.Delay(100); // The trace is asynchronous

            await TraceProcessor.Received(1).ProcessTraceAsync(
                Arg.Is<TraceEvent>(e =>
                    e.Settings.Mode == TraceMode.All &&
                    e.Settings.TargetType == TraceTargetType.Lime &&
                    e.Settings.Target == traceIdentity &&
                    e.Trace.User == UserIdentity.ToString() &&
                    e.Trace.Input == input &&
                    e.Trace.States.Count == 3 &&
                    e.Trace.States.ToArray()[0].Id == "root" &&
                    e.Trace.States.ToArray()[1].Id == "ping" &&
                    e.Trace.States.ToArray()[2].Id == "pong"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SecondBlockWithErrorShouldResultOnTwoTraces()
        {
            // Arrange
            var traceIdentity = new Identity(Guid.NewGuid().ToString(), "msging.net");
            var mode = "All";
            var targetType = "Lime";

            Message.Metadata = new Dictionary<string, string>
            {
                { "builder.trace.mode", mode },
                { "builder.trace.targetType", targetType },
                { "builder.trace.target", traceIdentity },
            };
            var messageType = "text/plain";
            var pingMessageContent = "Ping!!";
            var pongMessageContent = "Pong!!";

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
                        Input = new Input{
                            Bypass = true
                        },
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "x",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", pingMessageContent}
                                }
                            }
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "pong"
                            }
                        }
                    },
                    new State
                    {
                        Id = "pong",
                        Input = new Input(),
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", pongMessageContent}
                                }
                            }
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            try
            {
                await target.ProcessInputAsync(Message, flow, CancellationToken);
            }
            catch
            {

            }
            // Assert
            await Task.Delay(100); // The trace is asynchronous

            await TraceProcessor.Received(1).ProcessTraceAsync(
                Arg.Is<TraceEvent>(e =>
                    e.Settings.Mode == TraceMode.All &&
                    e.Settings.TargetType == TraceTargetType.Lime &&
                    e.Settings.Target == traceIdentity &&
                    e.Trace.User == UserIdentity.ToString() &&
                    e.Trace.Input == input &&
                    e.Trace.States.Count == 2 &&
                    e.Trace.States.ToArray()[0].Id == "root" &&
                    e.Trace.States.ToArray()[1].Id == "ping"),
                Arg.Any<CancellationToken>());
        }
    }
}