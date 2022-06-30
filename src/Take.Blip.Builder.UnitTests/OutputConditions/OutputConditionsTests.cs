using Lime.Messaging.Contents;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
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
            Message.Content = input;
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            await StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            await StateManager.DidNotReceive().SetStateIdAsync(Context, "marco", Arg.Any<CancellationToken>());
            await StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            await Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == pongMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithInvalidOutputConditionsShouldShouldFailAndNotChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "XPTO!" };
            Message.Content = input;
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            await StateManager.DidNotReceive().SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            await StateManager.DidNotReceive().SetStateIdAsync(Context, "marco", Arg.Any<CancellationToken>());
            await StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            await Sender
                .DidNotReceive()
                .SendMessageAsync(
                    Arg.Any<Message>(),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithMatchTextContextOutputConditionsShouldChangeStateAndSendMessage()
        {
            // Tests for Matches OutputConditions
            // Arrange
            var validInput = "Ping!";
            var messageType = "text/plain";
            var messageContent = "Pong!";

            var input = new PlainText() { Text = validInput };
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input), flow);

            await StateManager.Received(1).SetStateIdAsync(Arg.Any<IContext>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());

            await Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Any<CancellationToken>());
        }

        private Flow CreateVariableComparisonFlow(ConditionComparison condition, string variableName, string sentMessageType, string sentMessageContent)
        {
            return new Flow()
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
                                Order = 1,
                                Conditions = new Condition[]
                                {
                                    new Condition
                                    {
                                        Source = ValueSource.Context,
                                        Comparison = condition,
                                        Variable = variableName,
                                    }
                                },
                                StateId = "success"
                            }
                        }
                    },
                    new State
                    {
                        Id = "success",
                        InputActions = new Action[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", sentMessageType},
                                    {"content", sentMessageContent}
                                }
                            }
                        }
                    }
                }
            };
        }

        private Flow CreateVariableComparisonFlow(ConditionComparison condition, string variableName, string validInputValue, string sentMessageType, string sentMessageContent)
        {
            return new Flow()
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
                                Order = 1,
                                Conditions = new Condition[]
                                {
                                    new Condition
                                    {
                                        Source = ValueSource.Context,
                                        Comparison = condition,
                                        Variable = variableName,
                                        Values = new[] { validInputValue }
                                    }
                                },
                                StateId = "success"
                            }
                        }
                    },
                    new State
                    {
                        Id = "success",
                        InputActions = new Action[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", sentMessageType},
                                    {"content", sentMessageContent}
                                }
                            }
                        }
                    }
                }
            };
        }

        [Fact]
        public async Task FlowWithOutputConditionEqualsShouldChangeStateAndSendMessage()
        {
            // Tests for OutputConditions => Equals
            // Arrange
            var variableName = "MyVariable";
            var validInputValue = "Ping!";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText() { Text = validInputValue };
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.Equals, variableName, validInputValue, sentMessageType, sentMessageContent);
            var target = GetTarget();
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.Received(1).SetStateIdAsync(Context, "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
            await Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(sentMessageType)
                        && m.Content.ToString() == sentMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithOutputConditionEqualsShouldNotChangeState()
        {
            // Tests for OutputConditions => Equals
            // Arrange
            var variableName = "MyVariable";
            var validInputValue = "Ping!";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText() { Text = "Not Ping!" };
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.Equals, variableName, validInputValue, sentMessageType, sentMessageContent);
            var target = GetTarget();
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.DidNotReceive().SetStateIdAsync(Context, "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithOutputConditionContainsShouldChangeStateAndSendMessage()
        {
            // Tests for OutputConditions => Contains
            // Arrange
            var variableName = "MyVariable";
            var validInputValue = "ing";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.Contains, variableName, validInputValue, sentMessageType, sentMessageContent);
            var target = GetTarget();
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.Received(1).SetStateIdAsync(Arg.Any<IContext>(), "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
            await Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(sentMessageType)
                        && m.Content.ToString() == sentMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithOutputConditionContainsShouldNotChangeState()
        {
            // Tests for OutputConditions => Contains
            // Arrange
            var variableName = "MyVariable";
            var validInputValue = "Ping!";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText() { Text = "ing" };
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.Contains, variableName, validInputValue, sentMessageType, sentMessageContent);
            var target = GetTarget();
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.DidNotReceive().SetStateIdAsync(Context, "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithOutputConditionStartsShouldChangeStateAndSendMessage()
        {
            // Tests for OutputConditions => Starts
            // Arrange
            var variableName = "MyVariable";
            var validInputValue = "Pin";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.StartsWith, variableName, validInputValue, sentMessageType, sentMessageContent);
            var target = GetTarget();
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.Received(1).SetStateIdAsync(Context, "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
            await Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(sentMessageType)
                        && m.Content.ToString() == sentMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithOutputConditionStartsShouldNotChangeState()
        {
            // Tests for OutputConditions => Starts
            // Arrange
            var variableName = "MyVariable";
            var validInputValue = "Ping!";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText() { Text = "Pin" };
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.StartsWith, variableName, validInputValue, sentMessageType, sentMessageContent);
            var target = GetTarget();
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.DidNotReceive().SetStateIdAsync(Context, "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithOutputConditionEndsShouldChangeStateAndSendMessage()
        {
            // Tests for OutputConditions => Ends
            // Arrange
            var variableName = "MyVariable";
            var validInputValue = "g!";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.EndsWith, variableName, validInputValue, sentMessageType, sentMessageContent);
            var target = GetTarget();
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.Received(1).SetStateIdAsync(Context, "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
            await Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(sentMessageType)
                        && m.Content.ToString() == sentMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithOutputConditionEndsShouldNotChangeState()
        {
            // Tests for OutputConditions => Ends
            // Arrange
            var variableName = "MyVariable";
            var validInputValue = "Ping!";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText() { Text = "g!" };
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.EndsWith, variableName, validInputValue, sentMessageType, sentMessageContent);
            var target = GetTarget();
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.DidNotReceive().SetStateIdAsync(Context, "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithOutputConditionApproximateToShouldChangeStateAndSendMessage()
        {
            // Tests for OutputConditions => ApproximateTo
            // Arrange
            var variableName = "MyVariable";
            var validInputValue = "Ping!";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText() { Text = "Pamg!" };
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.ApproximateTo, variableName, validInputValue, sentMessageType, sentMessageContent);
            var target = GetTarget();
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.Received(1).SetStateIdAsync(Context, "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
            await Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(sentMessageType)
                        && m.Content.ToString() == sentMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithOutputConditionApproximateToShouldNotChangeState()
        {
            // Tests for OutputConditions => ApproximateTo
            // Arrange
            var variableName = "MyVariable";
            var validInputValue = "Ping!";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText() { Text = "Pamh!" };
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.ApproximateTo, variableName, validInputValue, sentMessageType, sentMessageContent);
            var target = GetTarget();
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.DidNotReceive().SetStateIdAsync(Context, "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithOutputConditionExistsShouldChangeStateAndSendMessage()
        {
            // Tests for OutputConditions => Exists
            // Arrange
            var variableName = "MyVariable";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.Exists, variableName, sentMessageType, sentMessageContent);
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.Received(1).SetStateIdAsync(Context, "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
            await Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(sentMessageType)
                        && m.Content.ToString() == sentMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithOutputConditionExistsShouldNotChangeState()
        {
            // Tests for OutputConditions => Exists
            // Arrange
            var variableName = "MyVariable";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText();
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.Exists, variableName, sentMessageType, sentMessageContent);
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.DidNotReceive().SetStateIdAsync(Context, "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithOutputConditionNotExistsShouldChangeStateAndSendMessage()
        {
            // Tests for OutputConditions => Exists
            // Arrange
            var variableName = "MyVariable";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText();
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.NotExists, variableName, sentMessageType, sentMessageContent);
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.Received(1).SetStateIdAsync(Context, "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
            await Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(sentMessageType)
                        && m.Content.ToString() == sentMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithOutputConditionNotExistsShouldNotChange()
        {
            // Tests for OutputConditions => Exists
            // Arrange
            var variableName = "MyVariable";
            var sentMessageType = "text/plain";
            var sentMessageContent = "Pong!";
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;

            var flow = CreateVariableComparisonFlow(ConditionComparison.NotExists, variableName, sentMessageType, sentMessageContent);
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(input.Text);
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Any<LazyInput>(), flow);
            await StateManager.DidNotReceive().SetStateIdAsync(Context, "success", Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(variableName, input.Text, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithStateVariableShouldSucceed()
        {
            // Arrange
            var variableName = "variableWithState";
            var state2 = "state2";

            var flow = GetVariableStateFlow(variableName);
            var target = GetTarget();

            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(state2);

            Message.Content = new PlainText() { Text = "hello" };

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            await StateManager.Received(1).SetStateIdAsync(Context, state2, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithoutStateVariableShouldError()
        {
            // Arrange
            var flow = GetVariableStateFlow("variableWithState");
            var target = GetTarget();

            Message.Content = new PlainText() { Text = "hello" };

            // Act
            await Assert.ThrowsAsync<OutputProcessingException>(
                async () => await target.ProcessInputAsync(Message, flow, CancellationToken)
            );

            // Assert
            await StateManager.DidNotReceive().SetStateIdAsync(Context, "state2", Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("inexistent state")]
        public async Task FlowWithInvalidStateVariableShouldError(string variableValue)
        {
            // Arrange
            var variableName = "variableWithState";

            var flow = GetVariableStateFlow(variableName);
            var target = GetTarget();

            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(variableValue);

            Message.Content = new PlainText() { Text = "hello" };

            // Act
            await Assert.ThrowsAsync<OutputProcessingException>(
                async () => await target.ProcessInputAsync(Message, flow, CancellationToken)
            );

            // Assert
            await StateManager.DidNotReceive().SetStateIdAsync(Context, "state2", Arg.Any<CancellationToken>());
        }

        private Flow GetVariableStateFlow(string variableName) => new Flow
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
                            Conditions = new[]
                            {
                                new Condition
                                {
                                    Source = ValueSource.Input,
                                    Comparison = ConditionComparison.Exists,
                                    Variable = variableName
                                }
                            },
                            StateId = "{{" + variableName + "}}"
                        }
                    }
                },
                new State
                {
                    Id = "state2"
                }
            }
        };
    }
}
