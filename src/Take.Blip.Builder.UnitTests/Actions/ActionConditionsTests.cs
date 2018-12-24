using Lime.Messaging.Contents;
using NSubstitute;
using SimpleInjector;
using System;
using System.Linq;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Models;
using Xunit;
using Action = Take.Blip.Builder.Models.Action;
using Input = Take.Blip.Builder.Models.Input;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ActionConditionsTests : FlowManagerTestsBase
    {
        public ActionConditionsTests()
        {
            ActionProvider = Substitute.For<IActionProvider>();
        }

        public override Container CreateContainer()
        {
            var container = base.CreateContainer();
            container.RegisterSingleton(ActionProvider);
            return container;
        }

        public IActionProvider ActionProvider { get; set; }

        private Flow CreateFlowWithTwoStates(string firstStateId, string firstStateContent, string secondStateId, string secondStateContent)
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
                                Conditions = new []
                                {
                                    new Condition
                                    {
                                        Values = new[] { firstStateContent }
                                    }
                                },
                                StateId = firstStateId
                            },
                            new Output
                            {
                                Conditions = new []
                                {
                                    new Condition
                                    {
                                        Values = new[] { secondStateContent }
                                    }
                                },
                                StateId = secondStateId
                            }
                        }
                    },
                    new State
                    {
                        Id = firstStateId,
                        Input = new Input(),
                        Outputs = new[]
                        {
                            new Output
                            {
                                Conditions = new []
                                {
                                    new Condition
                                    {
                                        Values = new[] { secondStateContent }
                                    }
                                },
                                StateId = secondStateId
                            }
                        }
                    },
                    new State
                    {
                        Id = secondStateId,
                        Input = new Input(),
                        Outputs = new[]
                        {
                            new Output
                            {
                                Conditions = new []
                                {
                                    new Condition
                                    {
                                        Values = new[] { firstStateContent }
                                    }
                                },
                                StateId = firstStateId
                            }
                        }
                    }
                }
            };
        }

        private void DefineInputActionsForDefinedState(Flow flow, string stateId, Action[] actions)
        {
            flow.States.First(state => state.Id == stateId).InputActions = actions;
        }

        private void DefineOutputActionsForDefinedState(Flow flow, string stateId, Action[] actions)
        {
            flow.States.First(state => state.Id == stateId).OutputActions = actions;
        }

        [Fact]
        public async Task FlowWithInputActionConditionsShouldSucceed()
        {
            // Arrange
            var firstStateId = "ping";
            var firstStateContent = "Ping!";
            var secondStateId = "pong";
            var secondStateContent = "Pong!";
            var flow = CreateFlowWithTwoStates(firstStateId, firstStateContent, secondStateId, secondStateContent);

            var inputActions = new[]
            {
                new Action
                {
                    Type = "ActionFistState",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Values = new[] { "Ping!" }
                        }
                    }
                },
                new Action
                {
                    Type = "OtherAction",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Values = new[] { "Other!" }
                        }
                    }
                },
            };
            DefineInputActionsForDefinedState(flow, firstStateId, inputActions);
            var input = new PlainText() { Text = "Ping!" };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(input, User, Application, flow, CancellationToken);

            // Assert
            ActionProvider.Received().Get("ActionFistState");
            ActionProvider.DidNotReceive().Get("OtherAction");
        }

        [Fact]
        public async Task FlowWithInputActionConditionsShouldFail()
        {
            // Arrange
            var firstStateId = "ping";
            var firstStateContent = "Ping!";
            var secondStateId = "pong";
            var secondStateContent = "Pong!";
            var flow = CreateFlowWithTwoStates(firstStateId, firstStateContent, secondStateId, secondStateContent);

            var inputActions = new[]
            {
                new Action
                {
                    Type = "ActionFistState",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Values = new[] { "XPTO!" }
                        }
                    }
                }
            };

            DefineInputActionsForDefinedState(flow, firstStateId, inputActions);
            var input = new PlainText() { Text = "Ping!" };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(input, User, Application, flow, CancellationToken);

            // Assert
            ActionProvider.DidNotReceiveWithAnyArgs().Get(Arg.Any<string>());
        }

        [Fact]
        public async Task FlowWithOutputActionConditionsShouldSucceed()
        {
            // Arrange
            var firstStateId = "ping";
            var firstStateContent = "Ping!";
            var secondStateId = "pong";
            var secondStateContent = "Pong!";
            var flow = CreateFlowWithTwoStates(firstStateId, firstStateContent, secondStateId, secondStateContent);

            var outputActions = new[]
            {
                new Action
                {
                    Type = "ActionFistState",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Values = new[] { "Ping!" }
                        }
                    }
                },
                new Action
                {
                    Type = "OtherAction",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Values = new[] { "Other!" }
                        }
                    }
                },
            };
            DefineOutputActionsForDefinedState(flow, "root", outputActions);
            var input = new PlainText() { Text = "Ping!" };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(input, User, Application, flow, CancellationToken);

            // Assert
            ActionProvider.Received().Get("ActionFistState");
            ActionProvider.DidNotReceive().Get("OtherAction");
        }

        [Fact]
        public async Task FlowWithOuputActionConditionsShouldFail()
        {
            // Arrange
            var firstStateId = "ping";
            var firstStateContent = "Ping!";
            var secondStateId = "pong";
            var secondStateContent = "Pong!";
            var flow = CreateFlowWithTwoStates(firstStateId, firstStateContent, secondStateId, secondStateContent);

            var outputActions = new[]
            {
                new Action
                {
                    Type = "ActionFistState",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Values = new[] { "XPTO!" }
                        }
                    }
                }
            };

            DefineOutputActionsForDefinedState(flow, "root", outputActions);
            var input = new PlainText() { Text = "Ping!" };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(input, User, Application, flow, CancellationToken);

            // Assert
            ActionProvider.DidNotReceiveWithAnyArgs().Get(Arg.Any<string>());
        }
    }
}