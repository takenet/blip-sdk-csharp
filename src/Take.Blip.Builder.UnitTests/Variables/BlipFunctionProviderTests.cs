using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using NSubstitute;
using Serilog;
using Shouldly;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Variables;
using Take.Blip.Client;
using Take.Blip.Client.Activation;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Variables
{
    public class BlipFunctionProviderTests : ContextTestsBase
    {
        private const string FUNCTION_NAME = "functest";
        private const string FUCTION_VALUE = "func run() {return 'teste'}";

        public BlipFunctionProviderTests()
        {
            Logger = Substitute.For<ILogger>();
            Sender = Substitute.For<ISender>();
            DocumentSerializer = new DocumentSerializer(new DocumentTypeResolver());

            Configuration = Substitute.For<IConfiguration>();
            Application = Substitute.For<Application>();
            InputContext = new Dictionary<string, object>();
            Context.InputContext.Returns(InputContext);

            var command = new Command()
            {
                Uri = new LimeUri($"/blipFunction/{Uri.EscapeDataString(FUNCTION_NAME)}"),
                Method = CommandMethod.Get,
                To = Node.Parse("postmaster@builder.msging.net")
            };

            var commandResult = new Command()
            {
                Status = CommandStatus.Success,
                Resource = PlainText.Parse(FUCTION_VALUE)
            };

            Sender.ProcessCommandAsync(Arg.Is<Command>(c => c.Id != null &&
                                                            c.Method.Equals(command.Method) &&
                                                            c.To.Equals(command.To) &&
                                                            c.Uri.Equals(command.Uri)),
                                      Arg.Any<CancellationToken>())
                .Returns(commandResult);
        }

        public IConfiguration Configuration { get; }

        public Application Application { get; }

        public IDictionary<string, object> InputContext { get; }

        public ILogger Logger { get; }

        public ISender Sender { get; }

        public IDocumentSerializer DocumentSerializer { get; }

        public IVariableProvider GetTarget()
        {
            return new BlipFunctionVariableProvider(Sender, DocumentSerializer, Logger);
        }

        [Fact]
        public async Task GetBlipFunctionValue()
        {
            //Arrange
            var target = GetTarget();

            //Act
            var actual = await target.GetVariableAsync(FUNCTION_NAME, Context, CancellationToken);

            //Assert
            actual.ShouldBe(FUCTION_VALUE);
        }
    }
}
