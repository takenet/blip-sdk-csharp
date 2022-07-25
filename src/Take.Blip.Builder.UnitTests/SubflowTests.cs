using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Take.Blip.Builder.Models;
using NSubstitute;
using Xunit;
using Action = Take.Blip.Builder.Models.Action;
using Input = Take.Blip.Builder.Models.Input;
using Newtonsoft.Json.Linq;
using System.Threading;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 4014

namespace Take.Blip.Builder.UnitTests
{
    public class SubflowTests : FlowManagerTestsBase, IDisposable
    {
        [Fact]
        public async Task FlowWithSubflowRedirectAndStateWithInputUserShouldSuccess()
        {
            // Arrange
            const string subflowShortName = "subflowShortName";
            
            var messageType = "text/plain";
            var messageContent = "Pong!";
            var messageContent2 = "Pong2!";
            var messageContentSubflow = "Pong Subflow!";

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

            var input2 = new Message() { From = UserIdentity.ToNode(), Content = new PlainText() { Text = "Ping Subflow!" } };
            var context2 = Substitute.For<IContext>();
            var lazyInput2 = new LazyInput(input2,
                UserIdentity,
                new BuilderConfiguration(),
                Substitute.For<IDocumentSerializer>(),
                Substitute.For<IEnvelopeSerializer>(),
                ArtificialIntelligenceExtension,
                CancellationToken);
            context2.Input.Returns(lazyInput2);

            var flow = CreateFlowObject(subflowShortName, messageType, messageContent, messageContent2);
            var subflow = CreateSubflowObject(messageType, messageContentSubflow, true);

            var target = GetTarget();
            subflow.Parent = flow;
            FlowLoader
                .LoadFlowAsync(FlowType.Subflow, Arg.Any<Flow>(), subflowShortName, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(subflow));

            context1.Flow.Returns(flow);
            context2.Flow.Returns(subflow);

            ContextProvider
               .CreateContext(Arg.Any<Identity>(), Arg.Any<Identity>(), Arg.Any<LazyInput>(), flow)
               .Returns(context1);
            ContextProvider
                .CreateContext(Arg.Any<Identity>(), Arg.Any<Identity>(), Arg.Any<LazyInput>(), subflow)
                .Returns(context2);

            // Act
            await target.ProcessInputAsync(input1, flow, CancellationToken);

            StateManager
                .GetParentStateIdAsync(Arg.Any<IContext>(), Arg.Any<CancellationToken>())
                .Returns("subflow:subflowTest");

            StateManager
                .GetStateIdAsync(Arg.Any<IContext>(), Arg.Any<CancellationToken>())
                .Returns("pingSubflow");

            await target.ProcessInputAsync(input2, subflow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input1.Content), flow);
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input2.Content), subflow);

            FlowLoader.Received(1).LoadFlowAsync(FlowType.Subflow, flow, subflowShortName, Arg.Any<CancellationToken>());

            StateManager.Received(1).SetStateIdAsync(context1, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(context1, "subflow:subflowTest", Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(context1, "root", Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(context1, "pingSubflow", Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(context2, "end", Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(context2, "ping2", Arg.Any<CancellationToken>());

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContentSubflow),
                    Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent2),
                    Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));
        }

        [Fact]
        public async Task FlowWithSubflowRedirectAndStateWithoutInputUserShouldSuccess()
        {
            // Arrange
            const string subflowShortName = "subflowShortName";
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            var messageType = "text/plain";
            var messageContent = "Pong!";
            var messageContent2 = "Pong2!";
            var messageContentSubflow = "Pong Subflow!";
            var flow = CreateFlowObject(subflowShortName, messageType, messageContent, messageContent2);
            var subflow = CreateSubflowObject(messageType, messageContentSubflow, false);

            var target = GetTarget();
            subflow.Parent = flow;
            FlowLoader
                .LoadFlowAsync(FlowType.Subflow, Arg.Any<Flow>(), subflowShortName, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(subflow));

            // Act
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input), flow);

            FlowLoader.Received(1).LoadFlowAsync(FlowType.Subflow, flow, subflowShortName, Arg.Any<CancellationToken>());

            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(Context, "subflow:subflowTest", Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(Context, "root", Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(Context, "pingSubflow", Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(Context, "end", Arg.Any<CancellationToken>());
            StateManager.Received(1).SetStateIdAsync(Context, "ping2", Arg.Any<CancellationToken>());

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContentSubflow),
                    Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent2),
                    Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));
        }

        [Fact]
        public async Task SubflowWithOldVersionShouldFail()
        {
            var subflowShortName = "subflowShortName";
            var flow = CreateFlowObject(subflowShortName, "text/plain", "messageContent", "messageContent2");
            var subflow = CreateSubflowObject("text/plain", "messageContent", false);

            var target = GetTarget();
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            subflow.Parent = flow;

            FlowLoader
                .LoadFlowAsync(FlowType.Subflow, Arg.Any<Flow>(), subflowShortName, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(subflow));

            subflow.Version = 0;

            // Act
            var exception = await Assert.ThrowsAsync<BuilderException>(
                async () => await target.ProcessInputAsync(Message, flow, CancellationToken)
            );
            Assert.IsType<ValidationException>(exception.InnerException);

            await Assert.ThrowsAsync<ValidationException>(
                async () => await target.ProcessInputAsync(Message, subflow, CancellationToken)
            );
        }

        [Fact]
        public async Task RedirectToSubflowWithNullParentShouldFail()
        {
            var subflowShortName = "subflowShortName";
            var flow = CreateFlowObject(subflowShortName, "text/plain", "messageContent", "messageContent2");
            var subflow = CreateSubflowObject("text/plain", "messageContent", false);

            var target = GetTarget();
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            subflow.Parent = null;

            FlowLoader
                .LoadFlowAsync(FlowType.Subflow, Arg.Any<Flow>(), subflowShortName, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(subflow));

            // Act
            var exception = await Assert.ThrowsAsync<BuilderException>(
                async () => await target.ProcessInputAsync(Message, flow, CancellationToken)
            );
            Assert.IsType<ArgumentNullException>(exception.InnerException);
            Assert.Equal($"Value cannot be null. (Parameter 'Error on return to parent flow of '{subflow.Id}'')", exception.InnerException.Message);
        }

        private static Flow CreateSubflowObject(string messageType, string messageContentSubflow, bool inputUserBeforeEnd)
        {
            return new Flow()
            {
                Id = Guid.NewGuid().ToString(),
                Type = FlowType.Subflow,
                Version = 2,
                States = new[]
                {
                    new State
                    {
                        Id = "root",
                        Root = true,
                        Input = new Input()
                        {
                            Bypass = true,
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "pingSubflow"
                            }
                        }
                    },
                    new State
                    {
                        Id = "pingSubflow",
                        Input = inputUserBeforeEnd ? new Input() : null,
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", messageContentSubflow}
                                }
                            }
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "end"
                            }
                        }
                    },
                    new State
                    {
                        Id = "end",
                        End = true
                    }
                }
            };
        }

        private static Flow CreateFlowObject(string subflowShortName, string messageType, string messageContent, string messageContent2)
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
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "subflow:subflowTest"
                            }
                        }
                    },
                    new State
                    {
                        Id = "subflow:subflowTest",
                        ExtensionData = new Dictionary<string, JToken>()
                        {
                            { "shortNameOfSubflow", new JValue(subflowShortName) }
                        },
                        Input = new Input()
                        {
                            Bypass = false
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
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", messageContent2}
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
