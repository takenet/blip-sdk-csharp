using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Serilog;
using Shouldly;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
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
        private static readonly Node BuilderAddress = Node.Parse($"postmaster@builder.msging.net");

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
                Uri = new LimeUri($"/functions/{Uri.EscapeDataString(FUNCTION_NAME)}"),
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
            var command = new Command()
            {
                Uri = new LimeUri($"/functions?functionName=teste"),
                To = BuilderAddress,
                Method = CommandMethod.Get
            };


            //Arrange
            var target = GetTarget();

            Sender.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(new Command()
            {
                Status = CommandStatus.Success,
                Resource = new DocumentCollection()
                {
                    ItemType = Function.MediaType,
                    Items = new Document[]
                    {
                        new Function()
                        {
                            FunctionContent = FUCTION_VALUE,
                            UserIdentity = "test",
                            FunctionDescription = "",
                            FunctionId  = Guid.NewGuid(),
                            FunctionName = "",
                            FunctionParameters = "",
                            TenantId = ""
                        }
                    }
                }
            });

            //Act
            var actual = await target.GetVariableAsync(FUNCTION_NAME, Context, CancellationToken);

            //Assert
            actual.ShouldBe(FUCTION_VALUE);
        }

        [Fact]
        public async Task ShouldReturnNullWithStatusDifferentSuccess()
        {
            var command = new Command()
            {
                Uri = new LimeUri($"/functions?functionName=teste"),
                To = BuilderAddress,
                Method = CommandMethod.Get
            };


            //Arrange
            var target = GetTarget();

            Sender.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(new Command()
            {
                Status = CommandStatus.Failure,
                Resource = new DocumentCollection()
                {
                    ItemType = Function.MediaType,
                    Items = new Document[]
                    {
                        new Function()
                        {
                            FunctionContent = FUCTION_VALUE,
                            UserIdentity = "test",
                            FunctionDescription = "",
                            FunctionId  = Guid.NewGuid(),
                            FunctionName = "",
                            FunctionParameters = "",
                            TenantId = ""
                        }
                    }
                }
            });

            //Act
            var actual = await target.GetVariableAsync(FUNCTION_NAME, Context, CancellationToken);

            //Assert
            actual.ShouldBe(null);
        }

        [Fact]
        public async Task ShouldReturnNullWithFunctionNull()
        {
            var command = new Command()
            {
                Uri = new LimeUri($"/functions?functionName=teste"),
                To = BuilderAddress,
                Method = CommandMethod.Get
            };


            //Arrange
            var target = GetTarget();

            Sender.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(new Command()
            {
                Status = CommandStatus.Success,
                Resource = new DocumentCollection()
                {
                    ItemType = Function.MediaType,
                    Items = new Document[]
                    {
                        new Function()
                        {
                        }
                    }
                }
            });

            //Act
            var actual = await target.GetVariableAsync(FUNCTION_NAME, Context, CancellationToken);

            //Assert
            actual.ShouldBe(null);
        }

        [Fact]
        public async Task ShouldThrowExceptionWithWrongObject()
        {
            var command = new Command()
            {
                Uri = new LimeUri($"/functions?functionName=teste"),
                To = BuilderAddress,
                Method = CommandMethod.Get
            };


            //Arrange
            var target = GetTarget();

            Sender.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(new Command()
            {
                Status = CommandStatus.Success,
                Resource = new DocumentCollection()
                {
                    ItemType = Function.MediaType,
                    Items = new Document[]
                    {}
                }
            });

            //Act
            target.GetVariableAsync(FUNCTION_NAME, Context, CancellationToken).ShouldThrow<Exception>();
        }
    }
}
