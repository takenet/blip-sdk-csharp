using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Take.Blip.Builder.Models;
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
            await target.ProcessInputAsync(input, User, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).GetContext(User, flow.Id, flow.Variables);
            StateManager.Received(1).SetStateIdAsync(flow.Id, User, "ping", CancellationToken);
            StateManager.Received(1).DeleteStateIdAsync(flow.Id, User, CancellationToken);
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m => 
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType) 
                        && m.Content.ToString() == messageContent), 
                    CancellationToken);
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
            await target.ProcessInputAsync(input, User, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).GetContext(User, flow.Id, flow.Variables);
            Context.Received(1).SetVariableAsync(variableName, input.Text, CancellationToken);
            StateManager.Received(0).SetStateIdAsync(Arg.Any<string>(), Arg.Any<Identity>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithContextConditionsShouldChangeStateAndSendMessage()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            var messageType = "text/plain";
            var pongMessageContent = "Pong!";
            var poloMessageContent = "Polo!";
            Context.GetVariableAsync("Word", CancellationToken).Returns(Task.FromResult(input.Text));
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
            await target.ProcessInputAsync(input, User, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(flow.Id, User, "ping", CancellationToken);
            StateManager.DidNotReceive().SetStateIdAsync(flow.Id, User, "marco", CancellationToken);
            StateManager.Received(1).DeleteStateIdAsync(flow.Id, User, CancellationToken);
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == pongMessageContent),
                    CancellationToken);
            Context.Received(1).SetVariableAsync("Word", input.Text, CancellationToken);
            Context.Received(2).GetVariableAsync("Word", CancellationToken);
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
            await target.ProcessInputAsync(input1, User, flow, CancellationToken);
            await target.ProcessInputAsync(input2, User, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(flow.Id, User, "ping", CancellationToken);
            StateManager.Received(1).SetStateIdAsync(flow.Id, User, "marco", CancellationToken);
            StateManager.Received(2).DeleteStateIdAsync(flow.Id, User, CancellationToken);
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == pongMessageContent),
                    CancellationToken);
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == poloMessageContent),
                    CancellationToken);
        }

        public void Dispose()
        {
            CancellationTokenSource.Dispose();
        }
    }
}
