using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using NSubstitute;
using SimpleInjector;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.Bucket;
using Xunit;
using Action = Take.Blip.Builder.Models.Action;
using Input = Take.Blip.Builder.Models.Input;

#pragma warning disable 4014

namespace Take.Blip.Builder.UnitTests
{
    public class FlowManagerTests : IDisposable
    {
        public FlowManagerTests()
        {
            BucketExtension = Substitute.For<IBucketExtension>();
            ArtificialIntelligenceExtension = Substitute.For<IArtificialIntelligenceExtension>();
            Sender = Substitute.For<ISender>();
            StorageManager = Substitute.For<IStorageManager>();
            CancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        }

        public IBucketExtension BucketExtension { get; set; }

        public IArtificialIntelligenceExtension ArtificialIntelligenceExtension { get; set; }

        public ISender Sender { get; set; }

        public IStorageManager StorageManager { get; set; }

        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public IFlowManager GetTarget()
        {
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;
            container.RegisterBuilder();
            container.RegisterSingleton(BucketExtension);
            container.RegisterSingleton(ArtificialIntelligenceExtension);
            container.RegisterSingleton(Sender);
            container.RegisterSingleton(StorageManager);
            return container.GetInstance<IFlowManager>();
        }

        [Fact]
        public async Task FlowWithoutConditionsShouldChangeStateAndSendMessage()
        {
            // Arrange
            var input = new PlainText() {Text = "Ping!"};
            var user = new Identity("user", "domain");
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
                                Settings = new Dictionary<string, object>
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
            await target.ProcessInputAsync(input, user, flow, CancellationToken);

            // Assert
            StorageManager.Received(1).SetStateIdAsync(flow.Id, user, "ping", CancellationToken);
            StorageManager.Received(1).DeleteStateIdAsync(flow.Id, user, CancellationToken);
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m => 
                        m.Id != null
                        && m.To.ToIdentity().Equals(user)
                        && m.Type.ToString().Equals(messageType) 
                        && m.Content.ToString() == messageContent), 
                    CancellationToken);
        }

        [Fact]
        public async Task FlowWithConditionsShouldChangeStateAndSendMessage()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            var user = new Identity("user", "domain");
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
                                        Value = "Marco!"
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
                                        Value = "Ping!"
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
                                Settings = new Dictionary<string, object>
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
                                Settings = new Dictionary<string, object>
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
            await target.ProcessInputAsync(input, user, flow, CancellationToken);

            // Assert
            StorageManager.Received(1).SetStateIdAsync(flow.Id, user, "ping", CancellationToken);
            StorageManager.DidNotReceive().SetStateIdAsync(flow.Id, user, "marco", CancellationToken);
            StorageManager.Received(1).DeleteStateIdAsync(flow.Id, user, CancellationToken);
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(user)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == pongMessageContent),
                    CancellationToken);
        }


        [Fact]
        public async Task FlowWithConditionsAndMultipleInputsShouldChangeStateAndSendMessage()
        {
            // Arrange
            var input1 = new PlainText() { Text = "Ping!" };
            var input2 = new PlainText() { Text = "Marco!" };
            var user = new Identity("user", "domain");
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
                                        Value = "Marco!"
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
                                        Value = "Ping!"
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
                                Settings = new Dictionary<string, object>
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
                                Settings = new Dictionary<string, object>
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
            await target.ProcessInputAsync(input1, user, flow, CancellationToken);
            await target.ProcessInputAsync(input2, user, flow, CancellationToken);

            // Assert
            StorageManager.Received(1).SetStateIdAsync(flow.Id, user, "ping", CancellationToken);
            StorageManager.Received(1).SetStateIdAsync(flow.Id, user, "marco", CancellationToken);
            StorageManager.Received(2).DeleteStateIdAsync(flow.Id, user, CancellationToken);
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(user)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == pongMessageContent),
                    CancellationToken);
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(user)
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
