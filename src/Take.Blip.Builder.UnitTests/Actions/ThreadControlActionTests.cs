using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Shouldly;
using SimpleInjector;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Actions.PassThreadControl;
using Take.Blip.Builder.Actions.TakeThreadControl;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils;
using Take.Blip.Client;
using Xunit;
using Action = Take.Blip.Builder.Models.Action;
using Input = Take.Blip.Builder.Models.Input;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ThreadControlActionTests : ActionTestsBase
    {
        private readonly ISender _sender;

        public ThreadControlActionTests()
        {
            _sender = Substitute.For<ISender>();
        }

        [Fact]
        public async Task PassWithTargetRoleShouldSendExpectedCommand()
        {
            var target = new PassThreadControlAction(_sender);
            var settings = JObject.FromObject(new
            {
                targetRole = "customer_service",
                metadata = "handoff-reason"
            });

            Command sentCommand = null;
            _sender.ProcessCommandAsync(Arg.Do<Command>(c => sentCommand = c), Arg.Any<CancellationToken>())
                .Returns(new Command { Status = CommandStatus.Success });

            await target.ExecuteAsync(Context, settings, CancellationToken);

            sentCommand.ShouldNotBeNull();
            sentCommand.Method.ShouldBe(CommandMethod.Set);
            sentCommand.Type.ToString().ShouldBe("application/json");
            sentCommand.To.ShouldBe(Node.Parse("postmaster@wa.gw.msging.net"));
            sentCommand.Uri.ToString().ShouldBe("/thread-control/pass");
            sentCommand.From.ShouldBeNull();
            string.IsNullOrWhiteSpace(sentCommand.Id).ShouldBeFalse();

            var serialized = SerializeCommand(sentCommand);
            serialized["resource"]["targetRole"]?.Value<string>().ShouldBe("customer_service");
            serialized["resource"]["metadata"]?.Value<string>().ShouldBe("handoff-reason");
        }

        [Fact]
        public async Task PassWithoutTargetRoleShouldNotSendTargetRole()
        {
            var target = new PassThreadControlAction(_sender);
            var settings = JObject.FromObject(new
            {
                metadata = "queue-default"
            });

            Command sentCommand = null;
            _sender.ProcessCommandAsync(Arg.Do<Command>(c => sentCommand = c), Arg.Any<CancellationToken>())
                .Returns(new Command { Status = CommandStatus.Success });

            await target.ExecuteAsync(Context, settings, CancellationToken);

            var serialized = SerializeCommand(sentCommand);
            serialized["resource"]["targetRole"].ShouldBeNull();
            serialized["resource"]["metadata"]?.Value<string>().ShouldBe("queue-default");
        }

        [Fact]
        public async Task PassWithInvalidTargetRoleShouldThrowValidationException()
        {
            var target = new PassThreadControlAction(_sender);
            var settings = JObject.FromObject(new
            {
                targetRole = "invalid-role",
                metadata = "x"
            });

            await Assert.ThrowsAsync<ValidationException>(() => target.ExecuteAsync(Context, settings, CancellationToken));

            await _sender.DidNotReceive().ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task TakeShouldSendOnlyMetadata()
        {
            var target = new TakeThreadControlAction(_sender);
            var settings = JObject.FromObject(new
            {
                metadata = "takeover-reason"
            });

            Command sentCommand = null;
            _sender.ProcessCommandAsync(Arg.Do<Command>(c => sentCommand = c), Arg.Any<CancellationToken>())
                .Returns(new Command { Status = CommandStatus.Success });

            await target.ExecuteAsync(Context, settings, CancellationToken);

            sentCommand.ShouldNotBeNull();
            sentCommand.Uri.ToString().ShouldBe("/thread-control/take");

            var serialized = SerializeCommand(sentCommand);
            serialized["resource"]["metadata"]?.Value<string>().ShouldBe("takeover-reason");
            serialized["resource"]["targetRole"].ShouldBeNull();
        }

        [Fact]
        public async Task TakeWithTargetRoleShouldThrowValidationException()
        {
            var target = new TakeThreadControlAction(_sender);
            var settings = JObject.FromObject(new
            {
                targetRole = "customer_service",
                metadata = "x"
            });

            await Assert.ThrowsAsync<ValidationException>(() => target.ExecuteAsync(Context, settings, CancellationToken));

            await _sender.DidNotReceive().ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData("Pass")]
        [InlineData("Take")]
        public async Task SerializedResourceShouldNeverContainForbiddenFields(string operation)
        {
            IAction target = operation == "Pass"
                ? new PassThreadControlAction(_sender)
                : new TakeThreadControlAction(_sender);

            var settings = operation == "Pass"
                ? JObject.FromObject(new { targetRole = "utility", metadata = "m" })
                : JObject.FromObject(new { metadata = "m" });

            Command sentCommand = null;
            _sender.ProcessCommandAsync(Arg.Do<Command>(c => sentCommand = c), Arg.Any<CancellationToken>())
                .Returns(new Command { Status = CommandStatus.Success });

            await target.ExecuteAsync(Context, settings, CancellationToken);

            var serialized = SerializeCommand(sentCommand);
            var serializedResource = serialized["resource"].ToString();
            serializedResource.Contains("identity").ShouldBeFalse();
            serializedResource.Contains("controlType").ShouldBeFalse();
            serializedResource.Contains("\"type\"").ShouldBeFalse();
        }

        [Fact]
        public async Task GatewayFailureResponseShouldThrow()
        {
            var target = new PassThreadControlAction(_sender);
            var settings = JObject.FromObject(new
            {
                targetRole = "utility",
                metadata = "escalate"
            });

            _sender.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>())
                .Returns(new Command
                {
                    Status = CommandStatus.Failure,
                    Reason = new Reason { Code = ReasonCodes.APPLICATION_ERROR, Description = "gateway failed" }
                });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => target.ExecuteAsync(Context, settings, CancellationToken));
            ex.Message.ShouldContain("gateway failed");
        }

        private static JObject SerializeCommand(Command command)
        {
            var json = JsonConvert.SerializeObject(command, LimeSerializerContainer.Serializer.Converters.ToArray());
            return JObject.Parse(json);
        }
    }

    public class ThreadControlFlowAndRegistrationTests : FlowManagerTestsBase
    {
        [Fact]
        public async Task MetadataInterpolationFromFlowManagerShouldReachActionResource()
        {
            var variableReplacer = Substitute.For<IVariableReplacer>();
            variableReplacer
                .ReplaceAsync(Arg.Any<string>(), Arg.Any<IContext>(), Arg.Any<CancellationToken>(), Arg.Any<string>())
                .Returns(call =>
                {
                    var value = call.ArgAt<string>(0);
                    return value.Replace("{{context.metadata}}", "resolved-metadata");
                });

            Sender.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>())
                .Returns(new Command { Status = CommandStatus.Success });

            var flow = new Flow
            {
                Id = System.Guid.NewGuid().ToString(),
                States = new[]
                {
                    new State
                    {
                        Id = "root",
                        Root = true,
                        Input = new Input(),
                        Outputs = new[]
                        {
                            new Output { StateId = "thread-control-state" }
                        }
                    },
                    new State
                    {
                        Id = "thread-control-state",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "PassThreadControl",
                                Settings = new JRaw(new JObject
                                {
                                    ["metadata"] = "{{context.metadata}}"
                                })
                            }
                        }
                    }
                }
            };

            var target = GetTarget(container => container.RegisterSingleton(() => variableReplacer));
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            await Sender.Received(1).ProcessCommandAsync(
                Arg.Is<Command>(c =>
                    c.Uri.ToString() == "/thread-control/pass" &&
                    SerializeCommand(c)["resource"]["metadata"]?.Value<string>() == "resolved-metadata"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public void ActionsShouldBeResolvableThroughActionProvider()
        {
            var container = CreateContainer();
            var actionProvider = container.GetInstance<IActionProvider>();

            actionProvider.Get("PassThreadControl").ShouldNotBeNull();
            actionProvider.Get("TakeThreadControl").ShouldNotBeNull();
        }

        private static JObject SerializeCommand(Command command)
        {
            var json = JsonConvert.SerializeObject(command, LimeSerializerContainer.Serializer.Converters.ToArray());
            return JObject.Parse(json);
        }
    }
}
