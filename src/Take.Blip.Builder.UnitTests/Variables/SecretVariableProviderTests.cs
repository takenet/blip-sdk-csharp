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
    public class SecretVariableProviderTests : ContextTestsBase
    {
        private const string SECRET_KEY = "mySecret";
        private const string SECRET_VALUE = "ZGRkZA==";
        private const string SECRET_VALUE_DECODED = "dddd";

        public SecretVariableProviderTests()
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
                Uri = new LimeUri($"/secrets/{Uri.EscapeDataString(SECRET_KEY)}"),
                Method = CommandMethod.Get,
                To = Node.Parse("postmaster@builder.msging.net")
            };

            var commandResult = new Command()
            {
                Status = CommandStatus.Success,
                Resource = PlainText.Parse(SECRET_VALUE)
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
            return new SecretVariableProvider(Sender, DocumentSerializer, Logger);
        }

        [Fact]
        public async Task GetSecretValue()
        {
            //Arrange
            var target = GetTarget();

            //Act
            var actual = await target.GetVariableAsync(SECRET_KEY, Context, CancellationToken);

            //Assert
            actual.ShouldBe(SECRET_VALUE_DECODED);
        }

        [Fact]
        public void GetSecretValueWithSendMessageShouldBeDeniedByRestriction()
        {
            //Arrange
            var target = GetTarget();

            //Act
            var attributeRestriction = target.GetType()
                    .GetCustomAttribute(typeof(VariableProviderRestrictionAttribute)) as VariableProviderRestrictionAttribute;

            var allowed = ContextBase.IsAllowedVariableProviderRestriction(attributeRestriction, "SendMessage");
            
            //Assert
            allowed.ShouldBeFalse();
        }

    }
}
