using Blip.Ai.Bot.Monitoring.Logging.Interface;
using Jint.Native;
using Lime.Messaging.Contents;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Serilog;
using Shouldly;
using SmartFormat.Core.Parsing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Content;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.Builder;
using Takenet.Iris.Messaging.Contents;
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
        public void RestoreBodyStringWithSecrets_ShouldRestoreSecretFields_AndSetSecretUrlBlipFlag()
        {
            var configuration = Substitute.For<IConfiguration>();
            var stateManager = Substitute.For<IStateManager>();
            var contextProvider = Substitute.For<IContextProvider>();
            var flowSemaphore = Substitute.For<IFlowSemaphore>();
            var actionProvider = Substitute.For<IActionProvider>();
            var sender = Substitute.For<ISender>();
            var documentSerializer = Substitute.For<IDocumentSerializer>();
            var envelopeSerializer = Substitute.For<IEnvelopeSerializer>();
            var artificialIntelligenceExtension = Substitute.For<IArtificialIntelligenceExtension>();
            var variableReplacer = Substitute.For<IVariableReplacer>();
            var logger = Substitute.For<ILogger>();
            var traceManager = Substitute.For<ITraceManager>();
            var userOwnerResolver = Substitute.For<IUserOwnerResolver>();
            var application = Substitute.For<Application>();
            var flowLoader = Substitute.For<IFlowLoader>();
            var flowSessionManager = Substitute.For<IFlowSessionManager>();
            var builderExceptions = Substitute.For<IAnalyzeBuilderExceptions>();
            var inputHandler = Substitute.For<IInputMessageHandlerAggregator>();
            var inputExpiration = Substitute.For<IInputExpirationCount>();
            var builderExtension = Substitute.For<IBuilderExtension>();
            var monitoring = Substitute.For<IBlipLogger>();

            var flowManager = new FlowManager(
                configuration,
                stateManager,
                contextProvider,
                flowSemaphore,
                actionProvider,
                sender,
                documentSerializer,
                envelopeSerializer,
                artificialIntelligenceExtension,
                variableReplacer,
                logger,
                traceManager,
                userOwnerResolver,
                application,
                flowLoader,
                flowSessionManager,
                builderExceptions,
                inputHandler,
                inputExpiration,
                builderExtension,
                 monitoring
            );
            var originalSettings = "{\"headers\":{\"Authorization\":\"{{secret.token}}\"},\"method\":\"POST\",\"body\":\"{\\\"id\\\":\\\"{{$guid}}\\\",\\\"uri\\\":\\\"{{resource.ping}}\\\",\\\"secret\\\":\\\"{{secret.mySecret}}\\\",\\\"normal\\\":\\\"{{resource.normal}}\\\"}\",\"uri\":\"{{secret.url}}\"}";
            var executedSettings = "{\"headers\":{\"Authorization\":\"real-token\"},\"method\":\"POST\",\"body\":\"{\\\"id\\\":\\\"{{$guid}}\\\",\\\"uri\\\":\\\"/ping\\\",\\\"secret\\\":\\\"***\\\",\\\"normal\\\":\\\"real-value\\\"}\",\"uri\":\"https://actual-url.com\"}";


            var expectedSecret = "{{secret.mySecret}}";
            var expectedUri = "/ping";


            // Use reflection se for método privado
            var method = typeof(FlowManager)
                .GetMethod("RestoreBodyStringWithSecrets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = method.Invoke(flowManager, new object[] { originalSettings, executedSettings }) as string;

            // Assert
            var resultObj = JObject.Parse(result);
            resultObj["SecretUrlBlip"].Value<bool>().ShouldBeTrue();

            var body = JObject.Parse(resultObj["body"].ToString());
            body["secret"].Value<string>().ShouldBe(expectedSecret);
            body["uri"].Value<string>().ShouldBe(expectedUri);
            body["normal"].Value<string>().ShouldBe("real-value");
        }

        [Fact]
        public void RestoreBodyStringWithSecrets_ShouldNotChange_WhenNoSecretsOrHttp()
        {
            // Arrange
            var originalSettings = "{\"headers\":{\"Authorization\":\"{{resource.token}}\"},\"method\":\"POST\",\"body\":\"{\\\"id\\\":\\\"{{$guid}}\\\",\\\"uri\\\":\\\"{{resource.ping}}\\\",\\\"normal\\\":\\\"{{resource.normal}}\\\"}\",\"uri\":\"{{resource.url}}\"}";

            var executedSettings = "{\"headers\":{\"Authorization\":\"real-token\"},\"method\":\"POST\",\"body\":\"{\\\"id\\\":\\\"{{$guid}}\\\",\\\"uri\\\":\\\"/ping\\\",\\\"normal\\\":\\\"real-value\\\"}\",\"uri\":\"https://actual-url.com\"}";

            var configuration = Substitute.For<IConfiguration>();
            var stateManager = Substitute.For<IStateManager>();
            var contextProvider = Substitute.For<IContextProvider>();
            var flowSemaphore = Substitute.For<IFlowSemaphore>();
            var actionProvider = Substitute.For<IActionProvider>();
            var sender = Substitute.For<ISender>();
            var documentSerializer = Substitute.For<IDocumentSerializer>();
            var envelopeSerializer = Substitute.For<IEnvelopeSerializer>();
            var artificialIntelligenceExtension = Substitute.For<IArtificialIntelligenceExtension>();
            var variableReplacer = Substitute.For<IVariableReplacer>();
            var logger = Substitute.For<ILogger>();
            var traceManager = Substitute.For<ITraceManager>();
            var userOwnerResolver = Substitute.For<IUserOwnerResolver>();
            var application = Substitute.For<Application>();
            var flowLoader = Substitute.For<IFlowLoader>();
            var flowSessionManager = Substitute.For<IFlowSessionManager>();
            var builderExceptions = Substitute.For<IAnalyzeBuilderExceptions>();
            var inputHandler = Substitute.For<IInputMessageHandlerAggregator>();
            var inputExpiration = Substitute.For<IInputExpirationCount>();
            var builderExtension = Substitute.For<IBuilderExtension>();
            var monitoring = Substitute.For<IBlipLogger>();

            var flowManager = new FlowManager(
                configuration,
                stateManager,
                contextProvider,
                flowSemaphore,
                actionProvider,
                sender,
                documentSerializer,
                envelopeSerializer,
                artificialIntelligenceExtension,
                variableReplacer,
                logger,
                traceManager,
                userOwnerResolver,
                application,
                flowLoader,
                flowSessionManager,
                builderExceptions,
                inputHandler,
                inputExpiration,
                builderExtension,
                monitoring
            );

            var method = typeof(FlowManager)
                .GetMethod("RestoreBodyStringWithSecrets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = method.Invoke(flowManager, new object[] { originalSettings, executedSettings }) as string;

            // Assert
            var resultObj = JObject.Parse(result);

            // secretUrlBlip should not be set
            resultObj.ContainsKey("secretUrlBlip").ShouldBeFalse();

            // body should not change from executed
            var body = JObject.Parse(resultObj["body"].ToString());
            body["normal"].Value<string>().ShouldBe("real-value");
            body["uri"].Value<string>().ShouldBe("/ping");
        }


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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", messageContent }
                                    }
                                )
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
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>(), Arg.Any<string>()).Returns(variableValue);

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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", messageContent }
                                    }
                                )
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
            Context.GetVariableAsync(variableName, Arg.Any<CancellationToken>(), Arg.Any<string>()).Returns(variableValue);

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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", messageContent }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", messageContent }
                                    }
                                )
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
        public async Task ProcessInputAsync_ShouldChangeActionType_WhenActionIsBlipFunction()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;

            var flow = new Flow()
            {
                Configuration = { },
                Id = Guid.NewGuid().ToString(),
                States = new[]
                {
            new State
            {
                Id = "root",
                Root = true,
                Input = new Input(),
                OutputActions = new[]
                {
                    new Action
                    {
                        Type = "ExecuteBlipFunction",
                        Settings = new JRaw(
                            new JObject()
                            {
                                { "function", "run" },
                                { "source", "function run(inputVariable1, inputVariable2) {\n    let a = inputVariable1.doesntExist;\n    let b = inputVariable2.doesntExist;\n\n    return a * b;\n}" },
                                { "outputVariable", "invalidScript" }
                            }
                        )
                    }
                }
            }
        }
            };

            var target = GetTarget();
            var functionDocument = new Function
            {
                FunctionContent = "function run(inputVariable1, inputVariable2) {\n    let a = inputVariable1.doesntExist;\n    let b = inputVariable2.doesntExist;\n\n    return a * b;\n}",
                UserIdentity = "teste",
                FunctionDescription = "",
                FunctionId = Guid.NewGuid(),
                FunctionName = "run",
                FunctionParameters = "",
                TenantId = ""
            };

            // Setup the BuilderExtension mock to return the function
            BuilderExtension.GetFunctionOnBlipFunctionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(functionDocument);

            // Setup Context mock for variable access
            Context.GetVariableAsync(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<string>())
                .Returns(Task.FromResult((string)null));

            // Act & Assert
            var exception = await target.ProcessInputAsync(Message, flow, CancellationToken).ShouldThrowAsync<ActionProcessingException>();
            exception.Message.ShouldContain("The processing of the action 'ExecuteScriptV2' has failed");
        }

        [Fact]
        public async Task FlowWithEmptyActionOnTrackEventShouldReturnFlowConstructionException()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
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
                OutputActions = new[]
                {
                    new Action
                    {
                        Type = "TrackEvent",
                        Settings = new JRaw(
                            new JObject()
                            {
                                { "category", "Variable doesn't exist" },
                                { "action", "{{variable.doesntExist}}" }
                            }
                        )
                    }
                }
            }
        }
            };

            // Setup the context to return null for the non-existent variable
            Context.GetVariableAsync("variable.doesntExist", Arg.Any<CancellationToken>(), Arg.Any<string>()).Returns((string)null);

            var target = GetTarget();

            // Act & Assert
            var exception = await target.ProcessInputAsync(Message, flow, CancellationToken).ShouldThrowAsync<FlowConstructionException>();
            exception.Message.ShouldContain("[FlowConstruction]");
            exception.Message.ShouldContain("TrackEvent");
        }

        [Fact]
        public async Task FlowWithInvalidValueOnRedirectShouldReturnFlowConstructionException()
        {
            // Arrange
            var input = new PlainText() { Text = "Test" };
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
                OutputActions = new[]
                {
                    new Action
                    {
                        Type = "Redirect",
                        Settings = new JRaw(
                            new JObject()
                            {
                                { "address", "{{variable.doesntExist}}" }
                            }
                        )
                    }
                }
            }
        }
            };

            // Setup the context to return null for the non-existent variable
            Context.GetVariableAsync("variable.doesntExist", Arg.Any<CancellationToken>(), Arg.Any<string>()).Returns(Task.FromResult((string)null));

            // Also setup the general variable retrieval to handle the parsing
            Context.GetVariableAsync(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<string>()).Returns(Task.FromResult((string)null));

            var target = GetTarget();

            // Act & Assert
            var exception = await target.ProcessInputAsync(Message, flow, CancellationToken).ShouldThrowAsync<FlowConstructionException>();
            exception.Message.ShouldContain("[FlowConstruction]");
            exception.Message.ShouldContain("Redirect");
        }

        [Fact]
        public async Task FlowWithInvalidStateIdOnOutputConditionsShouldReturnFlowConstructionException()
        {
            // Arrange
            var stateIdVariable = "{{variable.doesntExist}}";
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
                                StateId = stateIdVariable
                            }
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            var exception = await target.ProcessInputAsync(Message, flow, CancellationToken).ShouldThrowAsync<FlowConstructionException>();
            exception.Message.ShouldContain("[FlowConstruction]");
            exception.Message.ShouldContain("output condition to state");
        }

        [Fact]
        public async Task FlowWithInvalidScriptOnExecuteScriptShouldReturnFlowConstructionException()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
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
                OutputActions = new[]
                {
                    new Action
                    {
                        Type = "ExecuteScript",
                        Settings = new JRaw(
                            new JObject()
                            {
                                { "function", "run" },
                                { "source", "function run(inputVariable1, inputVariable2) {\n    let a = inputVariable1.doesntExist;\n    let b = inputVariable2.doesntExist;\n\n    return a * b;\n}" },
                                { "outputVariable", "invalidScript" }
                            }
                        )
                    }
                }
            }
        }
            };

            // Setup required mocks for the test to work properly
            Context.GetVariableAsync(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<string>()).Returns((string)null);

            var target = GetTarget();

            // Act & Assert
            var exception = await target.ProcessInputAsync(Message, flow, CancellationToken).ShouldThrowAsync<FlowConstructionException>();
            exception.Message.ShouldContain("[FlowConstruction]");
            exception.Message.ShouldContain("ExecuteScript");
        }

        [Fact]
        public async Task FlowWithTenTransitionsWithoutInterruptionsShouldReturnFlowConstructionException()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
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
                        StateId = "transition2"
                    }
                }
            },
            new State
            {
                Id = "transition2",
                Outputs = new[]
                {
                    new Output
                    {
                        StateId = "transition3"
                    }
                }
            },
            new State
            {
                Id = "transition3",
                Outputs = new[]
                {
                    new Output
                    {
                        StateId = "transition4"
                    }
                }
            },
            new State
            {
                Id = "transition4",
                Outputs = new[]
                {
                    new Output
                    {
                        StateId = "transition5"
                    }
                }
            },
            new State
            {
                Id = "transition5",
                Outputs = new[]
                {
                    new Output
                    {
                        StateId = "transition6"
                    }
                }
            },
            new State
            {
                Id = "transition6",
                Outputs = new[]
                {
                    new Output
                    {
                        StateId = "transition7"
                    }
                }
            },
            new State
            {
                Id = "transition7",
                Outputs = new[]
                {
                    new Output
                    {
                        StateId = "transition8"
                    }
                }
            },
            new State
            {
                Id = "transition8",
                Outputs = new[]
                {
                    new Output
                    {
                        StateId = "transition9"
                    }
                }
            },
            new State
            {
                Id = "transition9",
                Outputs = new[]
                {
                    new Output
                    {
                        StateId = "transition10"
                    }
                }
            },
            new State
            {
                Id = "transition10",
                Outputs = new[]
                {
                    new Output
                    {
                        StateId = "transition11"
                    }
                }
            },
            new State
            {
                Id = "transition11"
            }
        }
            };

            // Setup required mocks to prevent NullReferenceException
            Context.GetVariableAsync(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<string>())
                .Returns(Task.FromResult((string)null));

            var target = GetTarget();

            // Act & Assert
            var exception = await target.ProcessInputAsync(Message, flow, CancellationToken).ShouldThrowAsync<FlowConstructionException>();
            exception.Message.ShouldContain("[FlowConstruction]");
            exception.Message.ShouldContain("Max state transitions");
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
        public async Task FlowWithReplyMessageShouldSaveInContext()
        {
            // Arrange
            var input = new Reply()
            {
                Replied = new DocumentContainer
                {
                    Value = new PlainText { Text = "Replied" }
                },
                InReplyTo = new InReplyTo
                {
                    Id = Guid.NewGuid().ToString(),
                    Value = new PlainText { Text = "InReplyTo" }
                }
            };

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
            ContextProvider.Received(1).CreateContext(UserIdentity, ApplicationIdentity, Arg.Is<LazyInput>(i => i.Content == input.Replied.Value), flow);
            Context.Received(1).SetVariableAsync(variableName, ((PlainText)input.Replied.Value).Text, Arg.Any<CancellationToken>());
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", "Hello, {{contact.name}}" }
                                    }
                                )
                            }
                        }
                    }
                }
            };
            var target = GetTarget(container =>
            {
                container.RegisterSingleton<IContextProvider, ContextProvider>();
                container.RegisterSingleton<IServiceProvider>(() => container);
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
            Context.GetVariableAsync("Word", Arg.Any<CancellationToken>(), Arg.Any<string>()).Returns(Task.FromResult(input.Text));
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", pongMessageContent }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", poloMessageContent }
                                    }
                                )
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
            var inputOk = new Message() { From = UserIdentity.ToNode(), To = ApplicationIdentity.ToNode(), Content = new PlainText() { Text = "OK!" } };
            var inputNOk = new Message() { From = UserIdentity.ToNode(), To = ApplicationIdentity.ToNode(), Content = new PlainText() { Text = "NOK!" } };
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
                    if (variables.TryGetValue(key, out var value))
                        return value;
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "function", "run" },
                                        { "source", "function run() { return true; }" }, // Satisfying Input condition above
                                        { "outputVariable", "InputIsValid" }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", okMessageContent }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType},
                                        { "content", nokMessageContent }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", "failed to set variable" }
                                    }
                                )
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
            var inputOk = new Message() { From = UserIdentity.ToNode(), To = ApplicationIdentity.ToNode(), Content = new PlainText() { Text = "OK!" } };
            var inputNOk = new Message() { From = UserIdentity.ToNode(), To = ApplicationIdentity.ToNode(), Content = new PlainText() { Text = "NOK!" } };
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
                    if (variables.TryGetValue(key, out var value))
                        return value;
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "function", "run" },
                                        { "source", "function run(content) { return false; }" }, // Not satisfying Input condition above
                                        { "outputVariable", "InputIsValid" }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", okMessageContent }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", nokMessageContent }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", "failed to set variable" }
                                    }
                                )
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
            var input1 = new Message() { From = UserIdentity.ToNode(), To = ApplicationIdentity.ToNode(), Content = new PlainText() { Text = "Ping!" } };
            var context1 = Substitute.For<IContext>();
            var lazyInput1 = new LazyInput(input1,
                UserIdentity,
                new BuilderConfiguration(),
                Substitute.For<IDocumentSerializer>(),
                Substitute.For<IEnvelopeSerializer>(),
                ArtificialIntelligenceExtension,
                CancellationToken);
            context1.Input.Returns(lazyInput1);
            var input2 = new Message() { From = UserIdentity.ToNode(), To = ApplicationIdentity.ToNode(), Content = new PlainText() { Text = "Marco!" } };
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", pongMessageContent }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", poloMessageContent }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", messageContent }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", "This is not supposed to be received..." }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", messageContent }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", "This is not supposed to be received..." }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", messageContent }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", messageContent }
                                    }
                                )
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
                                Settings = new JRaw(
                                    new JObject()
                                    {
                                        { "type", messageType },
                                        { "content", messageContent }
                                    }
                                )
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

        #region ProcessCommandInputAsync

        [Fact]
        public async Task FlowWithAgentStateWithLocalCustomActionShouldAllowExecutionAsync()
        {
            // Arrange
            var stateId = "ai-agent:a64e773b-7233-4eca-90ad-321c1a42e051";
            var actionId = "action-inside-agent-a64e773b-7233-4eca-90ad-321c1a42e051";
            var variableName = "inputIsValid";
            var desiredScriptReturn = "true";

            var flow = new Flow()
            {
                Id = Guid.NewGuid().ToString(),
                States = new[]
                {
            new State
            {
                Id = stateId,
                Root = true,
                Input = new Input(),
                LocalCustomActions = new[]
                {
                    new Action
                    {
                        Id = actionId,
                        Type = "ExecuteScript",
                        Settings = new JRaw(
                            new JObject()
                            {
                                { "function", "run" },
                                { "source", $"function run() {{ return {desiredScriptReturn}; }}" }, // Satisfying Input condition above
                                { "outputVariable", variableName }
                            }
                        )
                    }
                }
            }
        }
            };

            var target = GetTarget();

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                To = "botidentity@msging.net",
                From = "fromaccountagent@msging.net",
            };

            // Mock the StateManager to return the expected state ID
            StateManager.GetStateIdAsync(Arg.Any<IContext>(), Arg.Any<CancellationToken>()).Returns(stateId);

            Context.GetContextVariableAsync(variableName, Arg.Any<CancellationToken>()).Returns(desiredScriptReturn);

            // Act
            var processCommandReturn = await target.ProcessCommandInputAsync(message, flow, stateId, actionId, CancellationToken.None);

            // Assert
            Context.Received(1).SetVariableAsync(Arg.Is(variableName), Arg.Is(desiredScriptReturn), Arg.Any<CancellationToken>());
            processCommandReturn.Count.ShouldBe(1);
            processCommandReturn.ShouldContainKeyAndValue(variableName, desiredScriptReturn);
        }

        [Fact]
        public async Task FlowWithAgentStateWithLocalCustomActionAndTwoVariablesOnOutputShouldAllowExecutionAsync()
        {
            // Arrange
            var stateId = "ai-agent:a64e773b-7233-4eca-90ad-321c1a42e051";
            var actionId = "action-inside-agent-a64e773b-7233-4eca-90ad-321c1a42e051";

            var statusCodeVariableName = "customerApiStatus";
            var statusCodeVariableValue = "202";

            var httpBodyReponseVariableName = "customerApiResponse";
            var httpBodyReponseVariableValue = "{}";

            var headers = new JObject
    {
        { "header1", "value1" },
        { "header2", "value2" }
    };

            var flow = new Flow()
            {
                Id = Guid.NewGuid().ToString(),
                States = new[]
                {
            new State
            {
                Id = stateId,
                Root = true,
                Input = new Input(),
                LocalCustomActions = new[]
                {
                    new Action
                    {
                        Id = actionId,
                        Type = "ProcessHttp",
                        Settings = new JRaw
                        (
                            new JObject()
                            {
                                { "responseStatusVariable", statusCodeVariableName },
                                { "responseBodyVariable", httpBodyReponseVariableName },
                                { "method", "GET" },
                                { "uri", "https://example.com/api/test" },
                                { "headers", headers }
                            }
                        )
                    }
                }
            }
        }
            };

            var target = GetTarget();

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                To = "botidentity@msging.net",
                From = "fromaccountagent@msging.net",
            };

            // Mock the StateManager to return the expected state ID
            StateManager.GetStateIdAsync(Arg.Any<IContext>(), Arg.Any<CancellationToken>()).Returns(stateId);

            Context.GetContextVariableAsync(httpBodyReponseVariableName, Arg.Any<CancellationToken>()).Returns(httpBodyReponseVariableValue);
            Context.GetContextVariableAsync(statusCodeVariableName, Arg.Any<CancellationToken>()).Returns(statusCodeVariableValue);

            using var httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.Accepted,
                Content = new StringContent(httpBodyReponseVariableValue)
            };

            HttpClient.SendAsync(Arg.Is<HttpRequestMessage>(m =>
                m.Method == HttpMethod.Get &&
                m.RequestUri.ToString() == "https://example.com/api/test"),
                Arg.Any<CancellationToken>())
                .Returns(httpResponse);

            // Act
            var processCommandReturn = await target.ProcessCommandInputAsync(message, flow, stateId, actionId, CancellationToken.None);

            // Assert
            Context
                .Received(1)
                .SetVariableAsync(Arg.Is(httpBodyReponseVariableName), Arg.Is(httpBodyReponseVariableValue), Arg.Any<CancellationToken>());
            Context
                .Received(1)
                .SetVariableAsync(Arg.Is(statusCodeVariableName), Arg.Is(statusCodeVariableValue), Arg.Any<CancellationToken>());

            processCommandReturn.Count.ShouldBe(2);
            processCommandReturn.ShouldContainKeyAndValue(httpBodyReponseVariableName, httpBodyReponseVariableValue);
            processCommandReturn.ShouldContainKeyAndValue(statusCodeVariableName, statusCodeVariableValue);
        }

        [Fact]
        public async Task FlowWithAgentStateWithLocalCustomActionAndOneOutputVariableInActionWithTwoPossibleOutputsShouldExecuteAsync()
        {
            // Arrange
            var stateId = "ai-agent:a64e773b-7233-4eca-90ad-321c1a42e051";
            var actionId = "action-inside-agent-a64e773b-7233-4eca-90ad-321c1a42e051";

            var statusCodeVariableName = "";
            var statusCodeVariableValue = "";

            var httpBodyReponseVariableName = "customerApiResponse";
            var httpBodyReponseVariableValue = "{}";

            // Representing empty headers
            var headers = new JObject();

            var flow = new Flow()
            {
                Id = Guid.NewGuid().ToString(),
                States = new[]
                {
                    new State
                    {
                        Id = stateId,
                        Root = true,
                        Input = new Input(),
                        LocalCustomActions = new[]
                        {
                            new Action
                            {
                                Id = actionId,
                                Type = "ProcessHttp",
                                Settings = new JRaw
                                (
                                    new JObject()
                                    {
                                        { "responseStatusVariable", statusCodeVariableName },
                                        { "responseBodyVariable", httpBodyReponseVariableName },
                                        { "method", "GET" },
                                        { "uri", "https://example.com/api/test" },
                                        { "headers", headers }
                                    }
                                )
                            }
                        }
                    }
                }
            };

            var target = GetTarget();

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                To = "botidentity@msging.net",
                From = "fromaccountagent@msging.net",
            };

            Context.GetContextVariableAsync(httpBodyReponseVariableName, Arg.Any<CancellationToken>()).Returns(httpBodyReponseVariableValue);

            HttpClient.SendAsync(Arg.Is<HttpRequestMessage>(m =>
                m.Method == HttpMethod.Get &&
                m.RequestUri.ToString() == "https://example.com/api/test"),
                Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.Accepted,
                    Content = new StringContent(httpBodyReponseVariableValue)
                });

            // Act
            var processCommandReturn = await target.ProcessCommandInputAsync(message, flow, stateId, actionId, CancellationToken.None);

            // Assert
            Context
                .Received(1)
                .SetVariableAsync(Arg.Is(httpBodyReponseVariableName), Arg.Is(httpBodyReponseVariableValue), Arg.Any<CancellationToken>());

            processCommandReturn.Count.ShouldBe(1);
            processCommandReturn.ShouldContainKeyAndValue(httpBodyReponseVariableName, httpBodyReponseVariableValue);
        }


        [Fact]
        public async Task FlowWithAgentStateWithLocalCustomActionWithoutOutputVariableShouldAllowExecutionAsync()
        {
            // Arrange
            var stateId = "ai-agent:a64e773b-7233-4eca-90ad-321c1a42e051";
            var actionId = "action-inside-agent-a64e773b-7233-4eca-90ad-321c1a42e051";
            var categoryName = "trackingA";
            var actionName = "valueTrackingA";

            var flow = new Flow()
            {
                Id = Guid.NewGuid().ToString(),
                States = new[]
                {
            new State
            {
                Id = stateId,
                Root = true,
                Input = new Input(),
                LocalCustomActions = new[]
                {
                    new Action
                    {
                        Id = actionId,
                        Type = "TrackEvent",
                        Settings = new JRaw(
                            new JObject()
                            {
                                { "extras", "" },
                                { "category", categoryName },
                                { "action", actionName }
                            }
                        )
                    }
                }
            }
        }
            };

            var target = GetTarget();

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                To = "botidentity@msging.net",
                From = "fromaccountagent@msging.net",
            };

            // Mock the StateManager to return the expected state ID
            StateManager.GetStateIdAsync(Arg.Any<IContext>(), Arg.Any<CancellationToken>()).Returns(stateId);

            // Act
            var processCommandReturn = await target.ProcessCommandInputAsync(message, flow, stateId, actionId, CancellationToken.None);

            // Assert
            processCommandReturn.ShouldBeNull();
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