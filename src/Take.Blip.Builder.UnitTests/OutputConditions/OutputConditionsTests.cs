using Lime.Messaging.Contents;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;
using Xunit;
using Action = Take.Blip.Builder.Models.Action;
using Input = Take.Blip.Builder.Models.Input;


namespace Take.Blip.Builder.UnitTests.OutputConditions
{
    public class OutputConditionsTests : FlowManagerTestsBase
    {
        [Fact]
        public async Task FlowWithOutputConditionsShouldChangeStateAndSendMessage()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
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
            await target.ProcessInputAsync(input, User, flow, CancellationToken);

            // Assert
            await StateManager.Received(1).SetStateIdAsync(flow.Id, User, "ping", Arg.Any<CancellationToken>());
            await StateManager.DidNotReceive().SetStateIdAsync(flow.Id, User, "marco", Arg.Any<CancellationToken>());
            await StateManager.Received(1).DeleteStateIdAsync(flow.Id, User, Arg.Any<CancellationToken>());
            await Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == pongMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithInvalidOutputConditionsShouldShouldFailAndNotChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "XPTO!" };
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
            await target.ProcessInputAsync(input, User, flow, CancellationToken);

            // Assert
            await StateManager.DidNotReceive().SetStateIdAsync(flow.Id, User, "ping", Arg.Any<CancellationToken>());
            await StateManager.DidNotReceive().SetStateIdAsync(flow.Id, User, "marco", Arg.Any<CancellationToken>());
            await StateManager.Received(1).DeleteStateIdAsync(flow.Id, User, Arg.Any<CancellationToken>());
            await Sender
                .DidNotReceive()
                .SendMessageAsync(
                    Arg.Any<Message>(),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithTextContextOutputConditionsShouldChangeStateAndSendMessage()
        {
            // Tests for OutputConditions (Equals, Contains, Starts, Ends)
            // Arrange
            var validInput = "Ping!";
            var messageType = "text/plain";
            var messageContent = "Pong!";

            var equalsInput = new PlainText() { Text = validInput };
            var equalsWithAccentInput = new PlainText() { Text = "Píng!" };
            var containsInput = new PlainText() { Text = "ing" };
            var startsInput = new PlainText() { Text = "Pin" };
            var endsInput = new PlainText() { Text = "g!" };
            var approximateInput = new PlainText() { Text = "Pamg!" };

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
                        },
                        Outputs = new Output[]
                        {
                            new Output
                            {
                                Order = 2,
                                Conditions = new Condition[]
                                {
                                    new Condition
                                    {
                                        Source = ValueSource.Context,
                                        Comparison = ConditionComparison.Equals,
                                        Variable = variableName,
                                        Values = new[] { validInput }
                                    }
                                },
                                StateId = "state2"
                            },
                            new Output
                            {
                                Order = 1,
                                Conditions = new Condition[]
                                {
                                    new Condition
                                    {
                                        Source = ValueSource.Context,
                                        Comparison = ConditionComparison.Contains,
                                        Variable = variableName,
                                        Values = new[] { validInput }
                                    }
                                },
                                StateId = "state2"
                            },
                            new Output
                            {
                                Order = 3,
                                Conditions = new Condition[]
                                {
                                    new Condition
                                    {
                                        Source = ValueSource.Context,
                                        Comparison = ConditionComparison.StartsWith,
                                        Variable = variableName,
                                        Values = new[] { validInput }
                                    }
                                },
                                StateId = "state2"
                            },
                            new Output
                            {
                                Order = 4,
                                Conditions = new Condition[]
                                {
                                    new Condition
                                    {
                                        Source = ValueSource.Context,
                                        Comparison = ConditionComparison.EndsWith,
                                        Variable = variableName,
                                        Values = new[] { validInput }
                                    }
                                },
                                StateId = "state2"
                            },
                            new Output
                            {
                                Order = 5,
                                Conditions = new Condition[]
                                {
                                    new Condition
                                    {
                                        Source = ValueSource.Context,
                                        Comparison = ConditionComparison.ApproximateTo,
                                        Variable = variableName,
                                        Values = new[] { validInput }
                                    }
                                },
                                StateId = "state2"
                            }
                        }
                    },
                    new State
                    {
                        Id = "state2",
                        InputActions = new Action[]
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

            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(validInput);

            // Act
            await target.ProcessInputAsync(equalsInput, User, flow, CancellationToken);
            await target.ProcessInputAsync(equalsWithAccentInput, User, flow, CancellationToken);
            await target.ProcessInputAsync(containsInput, User, flow, CancellationToken);
            await target.ProcessInputAsync(startsInput, User, flow, CancellationToken);
            await target.ProcessInputAsync(endsInput, User, flow, CancellationToken);
            await target.ProcessInputAsync(approximateInput, User, flow, CancellationToken);
            

            // Assert
            ContextProvider.Received(6).GetContext(User, flow.Id);

            await StateManager.Received(6).SetStateIdAsync(Arg.Any<string>(), Arg.Any<Identity>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

            await Context.Received(1).SetVariableAsync(variableName, equalsInput.Text, Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, equalsWithAccentInput.Text, Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, containsInput.Text, Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, startsInput.Text, Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, endsInput.Text, Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, approximateInput.Text, Arg.Any<CancellationToken>());

            await Sender
                .Received(6)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithMachTextContextOutputConditionsShouldChangeStateAndSendMessage()
        {
            // Tests for Maches OutputConditions
            // Arrange
            var validInput = "Ping!";
            var messageType = "text/plain";
            var messageContent = "Pong!";

            var input = new PlainText() { Text = validInput };

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
                        },
                        Outputs = new Output[]
                        {
                            new Output
                            {
                                Conditions = new Condition[]
                                {
                                    new Condition
                                    {
                                        Source = ValueSource.Context,
                                        Comparison = ConditionComparison.Matches,
                                        Variable = variableName,
                                        Values = new[] { "(Ping!)" }
                                    }
                                },
                                StateId = "state2"
                            }
                        }
                    },
                    new State
                    {
                        Id = "state2",
                        InputActions = new Action[]
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

            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(validInput);

            // Act
            await target.ProcessInputAsync(input, User, flow, CancellationToken);
            
            // Assert
            ContextProvider.Received(1).GetContext(User, flow.Id);

            await StateManager.Received(1).SetStateIdAsync(Arg.Any<string>(), Arg.Any<Identity>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());

            await Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Any<CancellationToken>());
        }

    }
}
