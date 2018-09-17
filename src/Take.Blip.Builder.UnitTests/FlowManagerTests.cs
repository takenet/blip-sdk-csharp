using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Models;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;
using Xunit;
using Action = Take.Blip.Builder.Models.Action;
using Input = Take.Blip.Builder.Models.Input;

#pragma warning disable 4014

namespace Take.Blip.Builder.UnitTests
{
    public class FlowManagerTests : FlowManagerTestsBase, IDisposable
    {
        [Fact]
        public async Task FlowWithoutConditionsShouldChangeStateAndSendMessage()
        {
            // Arrange
            var input = new PlainText() {Text = "Ping!"};
            var messageType = "text/plain";
            var messageContent = "Pong!";
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
            ContextProvider.Received(1).CreateContext(User, Application, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m => 
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType) 
                        && m.Content.ToString() == messageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithActionWithVariableShouldBeReplaced()
        {
            // Arrange
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
            ContextProvider.Received(1).CreateContext(User, Application, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == expectedMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithActionWithJsonVariableShouldBeEscapedAndReplaced()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            var messageType = "text/plain";
            var variableName = "variableName1";
            var variableValue = "{\"propertyName1\":\"propertyValue1\",\"propertyName2\":2}";
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
            ContextProvider.Received(1).CreateContext(User, Application, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == expectedMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithActionWithVariableThatNotExistsShouldBeReplacedByEmpty()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            var messageType = "text/plain";
            var messageContent = "Hello {{context.variableName1}}!";
            var expectedMessageContent = "Hello !";

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
            ContextProvider.Received(1).CreateContext(User, Application, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == expectedMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithInputVariableShouldSaveInContext()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            var variableName = "MyVariable";
            var flow = new Flow()
            {
                Id = Guid.NewGuid().ToString(),
                States = new[]
                {
                    new State
                    {
                        Id = "root",
                        Root = true,
                        Input = new Input
                        {
                            Variable = variableName
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(input, User, Application, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(User, Application, Arg.Is<LazyInput>(i => i.Content == input), flow);
            Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
            StateManager.Received(0).SetStateIdAsync(Arg.Any<IContext>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithContextConditionsShouldChangeStateAndSendMessage()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            var messageType = "text/plain";
            var pongMessageContent = "Pong!";
            var poloMessageContent = "Polo!";
            Context.GetVariableAsync("Word", Arg.Any<CancellationToken>()).Returns(Task.FromResult(input.Text));
            var flow = new Flow()
            {
                Id = Guid.NewGuid().ToString(),
                States = new[]
                {
                    new State
                    {
                        Id = "root",
                        Root = true,
                        Input = new Input()
                        {
                            Variable = "Word"
                            
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                Conditions = new []
                                {
                                    new Condition
                                    {
                                        Variable = "Word",
                                        Source = ValueSource.Context,
                                        Values = new[] { "Marco!" }
                                    }
                                },
                                StateId = "marco"
                            },
                            new Output
                            {
                                Conditions = new []
                                {
                                    new Condition
                                    {
                                        Variable = "Word",
                                        Source = ValueSource.Context,
                                        Values = new[] { "Ping!" }
                                    }
                                },
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
                                    {"content", pongMessageContent}
                                }
                            }
                        }
                    },
                    new State
                    {
                        Id = "marco",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", poloMessageContent}
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
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.DidNotReceive().SetStateIdAsync(Context, "marco", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == pongMessageContent),
                    Arg.Any<CancellationToken>());
            Context.Received(1).SetVariableAsync("Word", input.Text, Arg.Any<CancellationToken>());
            Context.Received(2).GetVariableAsync("Word", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithConditionsAndMultipleInputsShouldChangeStatesAndSendMessages()
        {
            // Arrange
            var input1 = new PlainText() { Text = "Ping!" };
            var input2 = new PlainText() { Text = "Marco!" };
            var messageType = "text/plain";
            var pongMessageContent = "Pong!";
            var poloMessageContent = "Polo!";
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
                                Conditions = new []
                                {
                                    new Condition
                                    {
                                        Values = new[] { "Marco!" }
                                    }
                                },
                                StateId = "marco"
                            },
                            new Output
                            {
                                Conditions = new []
                                {
                                    new Condition
                                    {
                                        Values = new[] { "Ping!" }
                                    }
                                },
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
                                    {"content", pongMessageContent}
                                }
                            }
                        }
                    },
                    new State
                    {
                        Id = "marco",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", poloMessageContent}
                                }
                            }
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(input1, User, Application, flow, CancellationToken);
            await target.ProcessInputAsync(input2, User, Application, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(Context, "marco", Arg.Any<CancellationToken>());
            StateManager.Received(2).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == pongMessageContent),
                    Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == poloMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithoutIntentConditionsShouldChangeStateAndSendMessage()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            var messageType = "text/plain";
            var messageContent = "This is my intent";
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
                                StateId = "my-intent",
                                Conditions = new[]
                                {
                                    new Condition()
                                    {
                                        Source = ValueSource.Intent,
                                        Values = new[]
                                        {
                                            "My intent"
                                        }
                                    }
                                }
                            },
                            new Output
                            {
                                StateId = "ping"
                            }
                        }
                    },
                    new State
                    {
                        Id = "my-intent",
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
                                    {"content", "This is not supposed to be received..."}
                                }
                            }
                        }
                    }
                }
            };

            ArtificialIntelligenceExtension
                .AnalyzeAsync(Arg.Is<AnalysisRequest>(r => r.Text == input.Text), Arg.Any<CancellationToken>())
                .Returns(new AnalysisResponse()
                {
                    Intentions = new []
                    {
                        new IntentionResponse
                        {
                            Name = "My intent",
                            Score = 1
                        },
                        new IntentionResponse
                        {
                            Name = "Other intent",
                            Score = 0.1
                        }
                    }
                });

            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(input, User, Application, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(User, Application, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "my-intent", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithoutEntityConditionsShouldChangeStateAndSendMessage()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            var messageType = "text/plain";
            var messageContent = "This is my entity";
            var entityName = "My entity name";
            var entityValue = "My entity value";

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
                                StateId = "my-entity",
                                Conditions = new[]
                                {
                                    new Condition()
                                    {
                                        Source = ValueSource.Entity,
                                        Entity = entityName,
                                        Values = new[]
                                        {
                                            entityValue
                                        }
                                    }
                                }
                            },
                            new Output
                            {
                                StateId = "ping"
                            }
                        }
                    },
                    new State
                    {
                        Id = "my-entity",
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
                                    {"content", "This is not supposed to be received..."}
                                }
                            }
                        }
                    }
                }
            };

            ArtificialIntelligenceExtension
                .AnalyzeAsync(Arg.Is<AnalysisRequest>(r => r.Text == input.Text), Arg.Any<CancellationToken>())
                .Returns(new AnalysisResponse()
                {
                    Entities = new[]
                    {
                        new EntityResponse()
                        {
                            Name = entityName,
                            Value = entityValue
                        },
                        new EntityResponse()
                        {
                            Name = "Other entity name",
                            Value = "Other entity value"
                        }
                    }
                });

            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(input, User, Application, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(User, Application, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "my-entity", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithTraceSettingsShouldCallTraceProcessor()
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
            TraceProcessor.Received(1).ProcessTraceAsync(
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

        public void Dispose()
        {
            CancellationTokenSource.Dispose();
        }
    }
}
