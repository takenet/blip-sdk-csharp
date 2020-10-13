using Lime.Messaging.Contents;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Actions.ProcessCommand;
using Take.Blip.Client;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ProcessCommandTests : ActionTestsBase
    {
        public ProcessCommandTests()
        {
            BlipClient = Substitute.For<ISender>();
            Context.Flow.Returns(new Builder.Models.Flow { Configuration = new Dictionary<string, string>() });
        }

        public ISender BlipClient { get; set; }

        private ProcessCommandAction GetTarget()
        {
            return new ProcessCommandAction(BlipClient, LimeSerializerContainer.EnvelopeSerializer);
        }

        [Fact]
        public async Task ProcessGetActionShouldSucceed()
        {
            // Arrange
            var command = new Command()
            {
                Id = EnvelopeId.NewId(),
                Method = CommandMethod.Get,
                Uri = new LimeUri("/ping")
            };

            var settings = JObject.FromObject(command, LimeSerializerContainer.Serializer);

            var variable = "responseBody";
            settings.TryAdd(nameof(variable), variable);

            var target = GetTarget();

            var responseCommand = new Command()
            {
                Method = CommandMethod.Get,
                Status = CommandStatus.Success,
                Resource = new JsonDocument()
            };

            BlipClient.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>()).Returns(responseCommand);

            // Act
            await target.ExecuteAsync(Context, settings, CancellationToken);

            // Assert
            await BlipClient.Received(1).ProcessCommandAsync(Arg.Is<Command>(c => c.Uri.Equals(command.Uri)), Arg.Any<CancellationToken>());

            await Context.Received(1).SetVariableAsync(variable, JsonConvert.SerializeObject(responseCommand, LimeSerializerContainer.Serializer.Converters.ToArray()), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessSetPlainTextActionShouldSucceed()
        {
            // Arrange
            var command = new Command()
            {
                Id = EnvelopeId.NewId(),
                Method = CommandMethod.Set,
                Uri = new LimeUri($"/contexts/{Context.UserIdentity}/somevariable"),
                Resource = new PlainText { Text = "some value" }
            };

            var settings = JObject.FromObject(command, LimeSerializerContainer.Serializer);

            var variable = "responseBody";
            settings.TryAdd(nameof(variable), variable);

            var target = GetTarget();

            var responseCommand = new Command()
            {
                Method = CommandMethod.Set,
                Status = CommandStatus.Success
            };

            BlipClient.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>()).Returns(responseCommand);

            // Act
            await target.ExecuteAsync(Context, settings, CancellationToken);

            // Assert
            await BlipClient.Received(1).ProcessCommandAsync(Arg.Is<Command>(c => c.Uri.Equals(command.Uri)), Arg.Any<CancellationToken>());

            await Context.Received(1).SetVariableAsync(variable, JsonConvert.SerializeObject(responseCommand, LimeSerializerContainer.Serializer.Converters.ToArray()), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessMergeContactActionShouldSucceed()
        {
            // Arrange
            var command = new Command()
            {
                Id = EnvelopeId.NewId(),
                Method = CommandMethod.Merge,
                Uri = new LimeUri($"/contacts"),
                Resource = new Contact() { Identity = Context.UserIdentity, Address = "Nowhere St." }
            };

            var settings = JObject.FromObject(command, LimeSerializerContainer.Serializer);

            var variable = "responseBody";
            settings.TryAdd(nameof(variable), variable);

            var target = GetTarget();

            var responseCommand = new Command()
            {
                Method = CommandMethod.Merge,
                Status = CommandStatus.Success
            };

            BlipClient.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>()).Returns(responseCommand);

            // Act
            await target.ExecuteAsync(Context, settings, CancellationToken);

            // Assert
            await BlipClient.Received(1).ProcessCommandAsync(Arg.Is<Command>(c => c.Uri.Equals(command.Uri)), Arg.Any<CancellationToken>());

            await Context.Received(1).SetVariableAsync(variable, JsonConvert.SerializeObject(responseCommand, LimeSerializerContainer.Serializer.Converters.ToArray()), Arg.Any<CancellationToken>());
        }
    }
}
