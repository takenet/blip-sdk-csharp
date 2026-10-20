using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Shouldly;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Actions.PassThreadControl;
using Take.Blip.Builder.Actions.TakeThreadControl;
using Take.Blip.Builder.Actions.ThreadControl;
using Take.Blip.Builder.Models;
using Take.Blip.Client;
using Xunit;
using Action = Take.Blip.Builder.Models.Action;
using Input = Take.Blip.Builder.Models.Input;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ThreadControlActionTests : ActionTestsBase
    {
        private readonly ISender _sender;
        private readonly IRedirectManager _redirectManager;

        public ThreadControlActionTests()
        {
            _sender = Substitute.For<ISender>();
            _redirectManager = Substitute.For<IRedirectManager>();
        }

        [Theory]
        [InlineData("customer_service")]
        [InlineData("marketing")]
        [InlineData("utility")]
        [InlineData("ctwa")]
        [InlineData("ai_agent")]
        [InlineData("escalation")]
        public async Task PassWithTargetRoleShouldSendExpectedCommand(string targetRole)
        {
            var target = new PassThreadControlAction(_sender);
            var settings = JObject.FromObject(new
            {
                controlType = "pass",
                targetRole,
                metadata = "handoff-reason"
            });

            Command sentCommand = null;
            _sender.ProcessCommandAsync(Arg.Do<Command>(c => sentCommand = c), Arg.Any<CancellationToken>())
                .Returns(new Command { Status = CommandStatus.Success });

            await target.ExecuteAsync(Context, settings, CancellationToken);

            AssertCommand(sentCommand, "pass");
            await _sender.Received(1).ProcessCommandAsync(sentCommand, CancellationToken);

            var serialized = SerializeCommand(sentCommand);
            serialized["resource"]["targetRole"]?.Value<string>().ShouldBe(targetRole);
            serialized["resource"]["metadata"]?.Value<string>().ShouldBe("handoff-reason");
        }

        [Fact]
        public async Task ReleaseShouldNotSendTargetRole()
        {
            var target = new PassThreadControlAction(_sender);
            var settings = JObject.FromObject(new
            {
                controlType = "release",
                metadata = "queue-default"
            });

            Command sentCommand = null;
            _sender.ProcessCommandAsync(Arg.Do<Command>(c => sentCommand = c), Arg.Any<CancellationToken>())
                .Returns(new Command { Status = CommandStatus.Success });

            await target.ExecuteAsync(Context, settings, CancellationToken);

            AssertCommand(sentCommand, "release");
            var serialized = SerializeCommand(sentCommand);
            serialized["resource"]["targetRole"].ShouldBeNull();
            serialized["resource"]["metadata"]?.Value<string>().ShouldBe("queue-default");
        }

        [Theory]
        [InlineData("pass", null)]
        [InlineData("release", "utility")]
        [InlineData("take", null)]
        public async Task InvalidPassSettingsShouldThrowBeforeSending(string controlType, string targetRole)
        {
            var target = new PassThreadControlAction(_sender);
            var settings = JObject.FromObject(new
            {
                controlType,
                targetRole,
                metadata = "x"
            });

            await Assert.ThrowsAsync<ValidationException>(() => target.ExecuteAsync(Context, settings, CancellationToken));

            await _sender.DidNotReceive().ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Continue the conversation")]
        public async Task TakeShouldSendCommandAndRedirect(string message)
        {
            var target = new TakeThreadControlAction(_sender, _redirectManager);
            var settings = JObject.FromObject(new
            {
                metadata = "takeover-reason",
                service = "support@msging.net",
                message
            });

            Redirect redirect = null;
            _redirectManager.RedirectUserAsync(
                Context, Arg.Do<Redirect>(r => redirect = r), CancellationToken)
                .Returns(Task.CompletedTask);
            Command sentCommand = null;
            _sender.ProcessCommandAsync(Arg.Do<Command>(c => sentCommand = c), Arg.Any<CancellationToken>())
                .Returns(new Command { Status = CommandStatus.Success });

            await target.ExecuteAsync(Context, settings, CancellationToken);

            AssertCommand(sentCommand, "take");
            await _sender.Received(1).ProcessCommandAsync(sentCommand, CancellationToken);

            var serialized = SerializeCommand(sentCommand);
            serialized["resource"]["metadata"]?.Value<string>().ShouldBe("takeover-reason");
            serialized["resource"]["targetRole"].ShouldBeNull();

            await _redirectManager.Received(1).RedirectUserAsync(
                Context, redirect, CancellationToken);
            redirect.ShouldNotBeNull();
            redirect.Address.ShouldBe(Node.Parse("support@msging.net"));
            if (string.IsNullOrWhiteSpace(message))
            {
                redirect.Context.ShouldBeNull();
            }
            else
            {
                redirect.Context.Type.ToString().ShouldBe("text/plain");
                ((PlainText)redirect.Context.Value).Text.ShouldBe(message);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task TakeWithoutServiceShouldThrowBeforeSending(string service)
        {
            var target = new TakeThreadControlAction(_sender, _redirectManager);
            var settings = JObject.FromObject(new
            {
                service,
                metadata = "x"
            });

            await Assert.ThrowsAsync<ValidationException>(() => target.ExecuteAsync(Context, settings, CancellationToken));

            await _sender.DidNotReceive().ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>());
            await _redirectManager.DidNotReceive().RedirectUserAsync(
                Arg.Any<IContext>(), Arg.Any<Redirect>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData("Pass")]
        [InlineData("Release")]
        [InlineData("Take")]
        public async Task SerializedResourceShouldContainOnlyContractFields(string operation)
        {
            var target = CreateAction(operation);

            var settings = CreateSettings(operation);
            settings["metadata"] = "identity controlType \"type\"";
            settings["identity"] = "other@wa.gw.msging.net";
            settings["type"] = "ignored";
            settings["userIdentity"] = "other@wa.gw.msging.net";
            settings["unknown"] = "ignored";
            if (operation == "Take")
            {
                settings["targetRole"] = "utility";
                settings["controlType"] = "pass";
            }

            Command sentCommand = null;
            _sender.ProcessCommandAsync(Arg.Do<Command>(c => sentCommand = c), Arg.Any<CancellationToken>())
                .Returns(new Command { Status = CommandStatus.Success });

            await target.ExecuteAsync(Context, settings, CancellationToken);

            AssertCommand(sentCommand, operation.ToLowerInvariant());
            var resource = (JObject)SerializeCommand(sentCommand)["resource"];
            var expectedFields = operation == "Pass"
                ? new[] { "controlType", "metadata", "targetRole", "userIdentity" }
                : new[] { "controlType", "metadata", "userIdentity" };
            resource.Properties().Select(p => p.Name).OrderBy(n => n)
                .ShouldBe(expectedFields);
            resource["metadata"].Value<string>().ShouldBe("identity controlType \"type\"");
        }

        [Theory]
        [InlineData("Pass", true)]
        [InlineData("Release", true)]
        [InlineData("Take", true)]
        [InlineData("Pass", false)]
        [InlineData("Release", false)]
        [InlineData("Take", false)]
        public async Task GatewayFailureResponseShouldThrowWithoutRedirecting(string operation, bool hasReason)
        {
            var target = CreateAction(operation);
            var settings = CreateSettings(operation);

            _sender.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>())
                .Returns(new Command
                {
                    Status = CommandStatus.Failure,
                    Reason = hasReason
                        ? new Reason { Code = ReasonCodes.APPLICATION_ERROR, Description = "gateway failed" }
                        : null
                });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => target.ExecuteAsync(Context, settings, CancellationToken));
            ex.Message.ShouldContain(hasReason
                ? $"{ReasonCodes.APPLICATION_ERROR} - gateway failed"
                : "No reason returned");
            await _redirectManager.DidNotReceive().RedirectUserAsync(
                Arg.Any<IContext>(), Arg.Any<Redirect>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData("targetRole", "invalid-role")]
        [InlineData("controlType", "invalid-operation")]
        public async Task InvalidEnumSettingsShouldThrowBeforeSending(string property, string value)
        {
            var target = new PassThreadControlAction(_sender);
            var settings = CreateSettings("Pass");
            settings[property] = value;

            await Assert.ThrowsAsync<JsonSerializationException>(
                () => target.ExecuteAsync(Context, settings, CancellationToken));

            await _sender.DidNotReceive().ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData("Pass", null)]
        [InlineData("Release", null)]
        [InlineData("Take", null)]
        [InlineData("Pass", "")]
        [InlineData("Release", "")]
        [InlineData("Take", "")]
        public async Task OptionalMetadataShouldOmitNullButPreserveEmptyStrings(string operation, string metadata)
        {
            var target = CreateAction(operation);
            var settings = CreateSettings(operation);
            if (metadata != null)
            {
                settings["metadata"] = metadata;
            }
            Command sentCommand = null;
            _sender.ProcessCommandAsync(Arg.Do<Command>(c => sentCommand = c), Arg.Any<CancellationToken>())
                .Returns(new Command { Status = CommandStatus.Success });

            await target.ExecuteAsync(Context, settings, CancellationToken);

            AssertCommand(sentCommand, operation.ToLowerInvariant());
            var resource = SerializeCommand(sentCommand)["resource"];
            if (metadata == null)
            {
                resource["metadata"].ShouldBeNull();
            }
            else
            {
                resource["metadata"].Value<string>().ShouldBe(metadata);
            }
        }

        [Theory]
        [InlineData("Pass")]
        [InlineData("Release")]
        [InlineData("Take")]
        public async Task SenderExceptionShouldPropagateWithoutRedirecting(string operation)
        {
            var target = CreateAction(operation);
            var exception = new InvalidOperationException("sender failed");
            _sender.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException<Command>(exception));

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(
                () => target.ExecuteAsync(Context, CreateSettings(operation), CancellationToken));

            actual.ShouldBeSameAs(exception);
            await _sender.Received(1).ProcessCommandAsync(Arg.Any<Command>(), CancellationToken);
            await _redirectManager.DidNotReceive().RedirectUserAsync(
                Arg.Any<IContext>(), Arg.Any<Redirect>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData("Pass")]
        [InlineData("Release")]
        [InlineData("Take")]
        public async Task SenderCancellationShouldPropagateWithoutRedirecting(string operation)
        {
            var target = CreateAction(operation);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            _sender.ProcessCommandAsync(Arg.Any<Command>(), cancellation.Token)
                .Returns(Task.FromCanceled<Command>(cancellation.Token));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => target.ExecuteAsync(Context, CreateSettings(operation), cancellation.Token));

            await _sender.Received(1).ProcessCommandAsync(Arg.Any<Command>(), cancellation.Token);
            await _redirectManager.DidNotReceive().RedirectUserAsync(
                Arg.Any<IContext>(), Arg.Any<Redirect>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task TakeShouldWaitForGatewaySuccessBeforeRedirecting()
        {
            var target = new TakeThreadControlAction(_sender, _redirectManager);
            var response = new TaskCompletionSource<Command>(TaskCreationOptions.RunContinuationsAsynchronously);
            _sender.ProcessCommandAsync(Arg.Any<Command>(), CancellationToken).Returns(response.Task);

            var execution = target.ExecuteAsync(Context, CreateSettings("Take"), CancellationToken);

            execution.IsCompleted.ShouldBeFalse();
            await _redirectManager.DidNotReceive().RedirectUserAsync(
                Arg.Any<IContext>(), Arg.Any<Redirect>(), Arg.Any<CancellationToken>());

            response.SetResult(new Command { Status = CommandStatus.Success });
            await execution;

            await _sender.Received(1).ProcessCommandAsync(Arg.Any<Command>(), CancellationToken);
            await _redirectManager.Received(1).RedirectUserAsync(Context, Arg.Any<Redirect>(), CancellationToken);
        }

        [Fact]
        public async Task RedirectExceptionShouldPropagateAfterTakingControl()
        {
            var target = new TakeThreadControlAction(_sender, _redirectManager);
            var exception = new InvalidOperationException("redirect failed");
            _sender.ProcessCommandAsync(Arg.Any<Command>(), CancellationToken)
                .Returns(new Command { Status = CommandStatus.Success });
            _redirectManager.RedirectUserAsync(Context, Arg.Any<Redirect>(), CancellationToken)
                .Returns(Task.FromException(exception));

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(
                () => target.ExecuteAsync(Context, CreateSettings("Take"), CancellationToken));

            actual.ShouldBeSameAs(exception);
            await _sender.Received(1).ProcessCommandAsync(Arg.Any<Command>(), CancellationToken);
            await _redirectManager.Received(1).RedirectUserAsync(Context, Arg.Any<Redirect>(), CancellationToken);
        }

        private IAction CreateAction(string operation)
        {
            return operation == "Take"
                ? (IAction)new TakeThreadControlAction(_sender, _redirectManager)
                : new PassThreadControlAction(_sender);
        }

        private static JObject CreateSettings(string operation)
        {
            return operation == "Take"
                ? JObject.FromObject(new { service = "support@msging.net" })
                : operation == "Release"
                    ? JObject.FromObject(new { controlType = "release" })
                    : JObject.FromObject(new { controlType = "pass", targetRole = "utility" });
        }

        private void AssertCommand(Command command, string controlType)
        {
            command.ShouldNotBeNull();
            command.Method.ShouldBe(CommandMethod.Set);
            command.Type.ToString().ShouldBe(ThreadControlCommandResource.MIME_TYPE);
            command.To.ShouldBe(Node.Parse("postmaster@wa.gw.msging.net"));
            command.Uri.ToString().ShouldBe("/thread-control");
            command.From.ShouldBeNull();
            string.IsNullOrWhiteSpace(command.Id).ShouldBeFalse();
            var resource = SerializeCommand(command)["resource"];
            resource["controlType"].Value<string>().ShouldBe(controlType);
            resource["userIdentity"].Value<string>().ShouldBe(UserIdentity.ToString());
        }

        private static JObject SerializeCommand(Command command)
        {
            var json = JsonConvert.SerializeObject(command, LimeSerializerContainer.Serializer.Converters.ToArray());
            return JObject.Parse(json);
        }
    }

    public class ThreadControlFlowAndRegistrationTests : FlowManagerTestsBase
    {
        [Theory]
        [InlineData("PassThreadControl")]
        [InlineData("TakeThreadControl")]
        public async Task MetadataInterpolationFromFlowManagerShouldReachActionResource(string actionType)
        {
            Message.Content = new PlainText { Text = "Take control" };
            const string metadata = "resolved \"metadata\"\nwith a \\ backslash";
            Context.GetVariableAsync("context.metadata", Arg.Any<CancellationToken>(), actionType)
                .Returns(metadata);
            var redirectManager = Substitute.For<IRedirectManager>();
            Command sentCommand = null;

            Sender.ProcessCommandAsync(Arg.Do<Command>(c => sentCommand = c), Arg.Any<CancellationToken>())
                .Returns(new Command { Status = CommandStatus.Success });

            var flow = new Flow
            {
                Id = "thread-control-flow",
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
                                Type = actionType,
                                Settings = new JRaw(new JObject
                                {
                                    ["targetRole"] = "utility",
                                    ["service"] = "support@msging.net",
                                    ["metadata"] = "{{context.metadata}}"
                                })
                            }
                        }
                    }
                }
            };

            var target = GetTarget(container => container.RegisterSingleton(() => redirectManager));
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            sentCommand.ShouldNotBeNull();
            sentCommand.Uri.ToString().ShouldBe("/thread-control");
            SerializeCommand(sentCommand)["resource"]["metadata"].Value<string>().ShouldBe(metadata);
            await Sender.Received(1).ProcessCommandAsync(sentCommand, Arg.Any<CancellationToken>());
        }

        [Fact]
        public void ActionsShouldBeResolvableThroughActionProvider()
        {
            var container = CreateContainer();
            var actionProvider = container.GetInstance<IActionProvider>();

            actionProvider.Get("PassThreadControl").ShouldBeOfType<PassThreadControlAction>();
            actionProvider.Get("TakeThreadControl").ShouldBeOfType<TakeThreadControlAction>();
        }

        private static JObject SerializeCommand(Command command)
        {
            var json = JsonConvert.SerializeObject(command, LimeSerializerContainer.Serializer.Converters.ToArray());
            return JObject.Parse(json);
        }
    }
}