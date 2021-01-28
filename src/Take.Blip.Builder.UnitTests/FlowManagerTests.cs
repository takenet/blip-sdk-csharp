using Lime.Messaging.Contents;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Storage;
using Take.Blip.Builder.Storage.Memory;
using Take.Blip.Client.Content;
using Takenet.Iris.Messaging.Resources;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;
using Xunit;
using Action = Take.Blip.Builder.Models.Action;
using Input = Take.Blip.Builder.Models.Input;
using ISender = Take.Blip.Client.ISender;

#pragma warning disable 4014

namespace Take.Blip.Builder.UnitTests
{
    public class FlowManagerTests : FlowManagerTestsBase, IDisposable
    {
        [Fact]
        public async Task FlowWithoutConditionsShouldChangeStateAndSendMessage()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));
        }

        [Fact]
        public async Task FlowWithActionWithVariableShouldBeReplaced()
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == expectedMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithActionWithJsonVariableShouldBeEscapedAndReplaced()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == expectedMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithActionWithVariableThatNotExistsShouldBeReplacedByEmpty()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == expectedMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithInputVariableShouldSaveInContext()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input), flow);
            Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
            StateManager.Received(0).SetStateIdAsync(Arg.Any<IContext>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithoutInputVariableShouldNotSaveInContext()
        {
            // Arrange
            Message.Content = new PlainText() { Text = "Ping!" };
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
                        Input = new Input(),
                        Outputs = new Output[]
                        {
                            new Output
                            {
                                StateId = "first"
                            }
                        }
                    },
                    new State
                    {
                        Id = "first",
                        Input = new Input
                        {
                            Bypass = true,
                            Variable = variableName
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == Message.Content), flow);
            Context.Received(0).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(Arg.Any<IContext>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task FlowWithContactVariableReplacementShouldGetContact()
        {
            // Arrange
            var input = new PlainText { Text = "Hi!" };
            var contactName = "Bob";
            var messageType = "text/plain";
            ContactExtension
                .GetAsync(UserIdentity, Arg.Any<CancellationToken>())
                .Returns(new Contact { Identity = UserIdentity, Name = contactName });
            Message.Content = input;

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
                                StateId = "welcome"
                            }
                        }
                    },
                    new State
                    {
                        Id = "welcome",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType },
                                    {"content", "Hello, {{contact.name}}"}
                                }
                            }
                        }
                    }
                }
            };
            var target = GetTarget(container =>
            {
                container.RegisterSingleton<IContextProvider, ContextProvider>();
                container.RegisterSingleton<IServiceProvider>(container);
            });

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContactExtension
                .Received()
                .GetAsync(
                    Arg.Is<Identity>(v => v == Message.From.ToIdentity()),
                    Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == $"Hello, {contactName}"),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithContextConditionsShouldChangeStateAndSendMessage()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.DidNotReceive().SetStateIdAsync(Context, "marco", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == pongMessageContent),
                    Arg.Any<CancellationToken>());
            Context.Received(1).SetVariableAsync("Word", input.Text, Arg.Any<CancellationToken>());
            Context.Received(2).GetVariableAsync("Word", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithInputContextConditionsSatisfiedShouldKeepStateAndWaitNextInput()
        {
            // Arrange
            var inputOk = new Message() { From = UserIdentity.ToNode(), Content = new PlainText() { Text = "OK!" } };
            var inputNOk = new Message() { From = UserIdentity.ToNode(), Content = new PlainText() { Text = "NOK!" } };
            var messageType = "text/plain";
            var okMessageContent = "OK";
            var nokMessageContent = "NOK";
            var variables = new Dictionary<string, string>();
            Context
                .When(c => c.SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()))
                .Do(callInfo =>
                {
                    var key = callInfo.ArgAt<string>(0);
                    var value = callInfo.ArgAt<string>(1);
                    variables[key] = value;
                });

            Context
                .GetVariableAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    var key = callInfo.ArgAt<string>(0);
                    if (variables.TryGetValue(key, out var value)) return value;
                    return null;
                });

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
                                StateId = "Start"
                            }
                        }
                    },
                    new State
                    {
                        Id = "Start",
                        Input = new Input()
                        {
                            Conditions = new []
                            {
                                new Condition
                                {
                                    Variable = "InputIsValid",
                                    Source = ValueSource.Context,
                                    Values = new[] { "true" }
                                }
                            }
                        },
                        InputActions = new []
                        {
                            new Action
                            {
                                Type = "ExecuteScript",
                                Settings = new JObject()
                                {
                                    { "function", "run" },
                                    { "source", "function run() { return true; }" }, // Satisfying Input condition above
                                    { "outputVariable", "InputIsValid" }
                                }
                            }
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                Conditions = new []
                                {
                                    new Condition
                                    {
                                        Variable = "InputIsValid",
                                        Source = ValueSource.Context,
                                        Values = new[] { "true" }
                                    }
                                },
                                StateId = "Ok"
                            },
                            new Output
                            {
                                Conditions = new []
                                {
                                    new Condition
                                    {
                                        Variable = "InputIsValid",
                                        Source = ValueSource.Context,
                                        Values = new[] { "false" }
                                    }
                                },
                                StateId = "NOk"
                            },
                            new Output
                            {
                                StateId = "error"
                            }
                        }
                    },
                    new State
                    {
                        Id = "Ok",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", okMessageContent }
                                }
                            }
                        }
                    },
                    new State
                    {
                        Id = "NOk",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", nokMessageContent }
                                }
                            }
                        }
                    },
                    new State
                    {
                        Id = "error",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", "failed to set variable" }
                                }
                            }
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(inputOk, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(Context, "Start", Arg.Any<CancellationToken>());
            StateManager.DidNotReceive().DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            StateManager.DidNotReceive().SetStateIdAsync(Context, "error", Arg.Any<CancellationToken>());
            StateManager.DidNotReceive().SetStateIdAsync(Context, "Ok", Arg.Any<CancellationToken>());
            StateManager.DidNotReceive().SetStateIdAsync(Context, "NOk", Arg.Any<CancellationToken>());

            Sender
                .DidNotReceive()
                .SendMessageAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());

            Context.Received(1).SetVariableAsync("InputIsValid", "true", Arg.Any<CancellationToken>());
            Context.Received(1).GetVariableAsync("InputIsValid", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithInputContextConditionsNotSatisfiedShouldChangeStateAndSendMessage()
        {
            // Arrange
            var inputOk = new Message() { From = UserIdentity.ToNode(), Content = new PlainText() { Text = "OK!" } };
            var inputNOk = new Message() { From = UserIdentity.ToNode(), Content = new PlainText() { Text = "NOK!" } };
            var messageType = "text/plain";
            var okMessageContent = "OK";
            var nokMessageContent = "NOK";
            var variables = new Dictionary<string, string>();
            Context
                .When(c => c.SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()))
                .Do(callInfo =>
                {
                    var key = callInfo.ArgAt<string>(0);
                    var value = callInfo.ArgAt<string>(1);
                    variables[key] = value;
                });

            Context
                .GetVariableAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    var key = callInfo.ArgAt<string>(0);
                    if (variables.TryGetValue(key, out var value)) return value;
                    return null;
                });

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
                                StateId = "Start"
                            }
                        }
                    },
                    new State
                    {
                        Id = "Start",
                        Input = new Input()
                        {
                            Conditions = new []
                            {
                                new Condition
                                {
                                    Variable = "InputIsValid",
                                    Source = ValueSource.Context,
                                    Values = new[] { "true" }
                                }
                            }
                        },
                        InputActions = new []
                        {
                            new Action
                            {
                                Type = "ExecuteScript",
                                Settings = new JObject()
                                {
                                    { "function", "run" },
                                    { "source", "function run(content) { return false; }" }, // Not satisfying Input condition above
                                    { "outputVariable", "InputIsValid" }
                                }
                            }
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                Conditions = new []
                                {
                                    new Condition
                                    {
                                        Variable = "InputIsValid",
                                        Source = ValueSource.Context,
                                        Values = new[] { "true" }
                                    }
                                },
                                StateId = "Ok"
                            },
                            new Output
                            {
                                Conditions = new []
                                {
                                    new Condition
                                    {
                                        Variable = "InputIsValid",
                                        Source = ValueSource.Context,
                                        Values = new[] { "false" }
                                    }
                                },
                                StateId = "NOk"
                            },
                            new Output
                            {
                                StateId = "error"
                            }
                        }
                    },
                    new State
                    {
                        Id = "Ok",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", okMessageContent }
                                }
                            }
                        }
                    },
                    new State
                    {
                        Id = "NOk",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", nokMessageContent }
                                }
                            }
                        }
                    },
                    new State
                    {
                        Id = "error",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", "failed to set variable" }
                                }
                            }
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(inputNOk, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(Context, "Start", Arg.Any<CancellationToken>());
            StateManager.DidNotReceive().SetStateIdAsync(Context, "error", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(Context, "NOk", Arg.Any<CancellationToken>());
            StateManager.DidNotReceive().SetStateIdAsync(Context, "Ok", Arg.Any<CancellationToken>());

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == nokMessageContent),
                    Arg.Any<CancellationToken>());
            Context.Received(1).SetVariableAsync("InputIsValid", "false", Arg.Any<CancellationToken>());
            Context.Received(3).GetVariableAsync("InputIsValid", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithConditionsAndMultipleInputsShouldChangeStatesAndSendMessages()
        {
            // Arrange
            var input1 = new Message() { From = UserIdentity.ToNode(), Content = new PlainText() { Text = "Ping!" } };
            var context1 = Substitute.For<IContext>();
            var lazyInput1 = new LazyInput(input1,
                UserIdentity,
                new BuilderConfiguration(),
                Substitute.For<IDocumentSerializer>(),
                Substitute.For<IEnvelopeSerializer>(),
                ArtificialIntelligenceExtension,
                CancellationToken);
            context1.Input.Returns(lazyInput1);
            var input2 = new Message() { From = UserIdentity.ToNode(), Content = new PlainText() { Text = "Marco!" } };
            var context2 = Substitute.For<IContext>();
            var lazyInput2 = new LazyInput(input2,
                UserIdentity,
                new BuilderConfiguration(),
                Substitute.For<IDocumentSerializer>(),
                Substitute.For<IEnvelopeSerializer>(),
                ArtificialIntelligenceExtension,
                CancellationToken);
            context2.Input.Returns(lazyInput2);
            ContextProvider
                .CreateContext(Arg.Any<Identity>(), Arg.Any<Identity>(), lazyInput1, Arg.Any<Flow>())
                .Returns(context1);
            ContextProvider
                .CreateContext(Arg.Any<Identity>(), Arg.Any<Identity>(), lazyInput2, Arg.Any<Flow>())
                .Returns(context2);

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
            await target.ProcessInputAsync(input1, flow, CancellationToken);
            await target.ProcessInputAsync(input2, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(Context, "marco", Arg.Any<CancellationToken>());
            StateManager.Received(2).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == pongMessageContent),
                    Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == poloMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithoutIntentConditionsShouldChangeStateAndSendMessage()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
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
                    Intentions = new[]
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "my-intent", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithoutEntityConditionsShouldChangeStateAndSendMessage()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "my-entity", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task TimeoutOnActionShouldOverrideDefaultConfiguration()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            var messageType = "text/plain";
            var messageContent = "Pong!";

            var timeout = TimeSpan.FromMilliseconds(256);
            var fakeSender = new FakeSender(timeout + timeout);
            Sender = fakeSender;
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
                                Timeout = timeout.TotalSeconds,
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
            var exception = await target.ProcessInputAsync(Message, flow, CancellationToken).ShouldThrowAsync<ActionProcessingException>();
            exception.Message.ShouldBe($"The processing of the action 'SendMessage' has timed out after {timeout.TotalMilliseconds} ms");
            fakeSender.SentMessages.ShouldBeEmpty();
        }

        [Fact]
        public async Task ActionWithInvalidSettingShouldBreakProcessing()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            var messageType = "application/json";
            var messageContent = "NOT A JSON";
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
            await target
                .ProcessInputAsync(Message, flow, CancellationToken)
                .ShouldThrowAsync<ActionProcessingException>();
        }

        [Fact]
        public async Task ActionWithInvalidSettingShouldNotBreakProcessingWhenContinueOnErrorIsTrue()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            var messageType = "application/json";
            var messageContent = "NOT A JSON";
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
                                ContinueOnError = true,
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
        }

        #region TemporaryInput
        [Fact]
        public async Task FlowWithTemporaryInputShouldScheduleAInputExpirationTimeMessage()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            var messageType = InputExpiration.MIME_TYPE;
            var messageContent = new InputExpiration() { Identity = UserIdentity };
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
                        Input = new Input()
                        {
                            Expiration = TimeSpan.FromMinutes(1)
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            SchedulerExtension
                .Received(1)
                .ScheduleMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(ApplicationIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content is InputExpiration
                        && UserIdentity.Equals((m.Content as InputExpiration).Identity)),
                    Arg.Any<DateTimeOffset>(),
                    Arg.Any<Node>(),
                    Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));
        }

        //When content input temporary is null or empty
        [Fact]
        public async Task FlowWithTemporaryInputWithEmptyContentShouldThrowsAException()
        {
            // Arrange
            var input = new InputExpiration() { Identity = null };
            Message.Content = input;
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
                        Input = new Input()
                        {
                            Expiration = TimeSpan.FromMinutes(1)
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            Func<Task> processInputAsync = async () => await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            processInputAsync.ShouldThrow<ArgumentException>();
        }

        //When user send other input after one temporary input
        [Fact]
        public async Task FlowWithTemporaryInputShouldCancelScheduleWhenUserSendOtherInput()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            var messageType = InputExpiration.MIME_TYPE;
            var messageContent = new InputExpiration() { Identity = UserIdentity };
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
                        Input = new Input()
                        {
                            Expiration = TimeSpan.FromMinutes(1)
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "ping2"
                            }
                        }
                    },
                    new State
                    {
                        Id = "ping2",
                        Input = new Input()
                    }
                }
            };
            var target = GetTarget();
            SchedulerExtension
               .GetScheduledMessageAsync(Arg.Any<string>(), Arg.Any<Node>(), Arg.Any<CancellationToken>())
               .Returns(new Schedule
               {
                   Name = $"{UserIdentity}-inputexpirationtime",
                   Message = Message,
                   Status = ScheduleStatus.Scheduled,
                   When = DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(1))
               });

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);
            StateManager.GetStateIdAsync(Arg.Any<IContext>(), Arg.Any<CancellationToken>()).Returns("ping");
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(2).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input), flow);
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(Context, "ping2", Arg.Any<CancellationToken>());
            SchedulerExtension
                .Received(1)
                .ScheduleMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id.Equals($"{UserIdentity}-inputexpirationtime")
                        && m.To.ToIdentity().Equals(ApplicationIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content is InputExpiration
                        && UserIdentity.Equals((m.Content as InputExpiration).Identity)),
                    Arg.Any<DateTimeOffset>(),
                    Arg.Any<Node>(),
                    Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));
            SchedulerExtension
                .Received(1)
                .CancelScheduledMessageAsync(
                    Arg.Is<string>(s => s.Equals($"{UserIdentity}-inputexpirationtime")),
                    Arg.Any<Node>(),
                    Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));

            StateManager = Substitute.For<IStateManager>();
        }
        #endregion

        public void Dispose()
        {
            CancellationTokenSource.Dispose();
        }

        private class FakeSender : ISender
        {
            private readonly TimeSpan _delay;

            public FakeSender(TimeSpan delay)
            {
                _delay = delay;
                SentMessages = new List<Message>();
            }

            public List<Message> SentMessages { get; }


            public async Task SendMessageAsync(Message message, CancellationToken cancellationToken)
            {
                await Task.Delay(_delay, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                SentMessages.Add(message);
            }

            public Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task SendCommandAsync(Command command, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<Command> ProcessCommandAsync(Command requestCommand, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}