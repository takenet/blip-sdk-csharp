using Xunit;
using Take.Blip.Builder.Actions.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Take.Blip.Client.Extensions.Builder.Checkout;
using Take.Blip.Client;
using Serilog;
using NSubstitute;
using Take.Blip.Builder.Actions.Checkout.Settings;
using Lime.Protocol;
using System.Threading;
using Lime.Messaging.Contents;
using Take.Blip.Builder.Models;
using Lime.Protocol.Serialization;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Takenet.Iris.Messaging.Resources.Checkout;

namespace Take.Blip.Builder.Actions.Checkout.Tests
{
    public class CheckoutUnitTests
    {
        private readonly ICheckoutExtension _checkoutExtension;
        private readonly ISender _sender;
        private readonly ILogger _logger;

        private readonly BuilderConfiguration _builderConfiguration;
        private readonly IDocumentSerializer _documentSerializer;
        private readonly IEnvelopeSerializer _envelopeSerializer;
        private readonly IArtificialIntelligenceExtension _artificialIntelligenceExtension;

        private readonly CheckoutAction _checkoutAction;

        public CheckoutUnitTests()
        {
            _checkoutExtension = Substitute.For<ICheckoutExtension>();
            _sender = Substitute.For<ISender>();
            _logger = Substitute.For<ILogger>();

            _builderConfiguration = Substitute.For<BuilderConfiguration>();
            _documentSerializer = Substitute.For<IDocumentSerializer>();
            _envelopeSerializer = Substitute.For<IEnvelopeSerializer>();
            _artificialIntelligenceExtension = Substitute.For<IArtificialIntelligenceExtension>();

            _checkoutAction = new CheckoutAction(_checkoutExtension, _sender, _logger);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCreateCheckoutLinkAndSendMessage()
        {
            // Arrange  
            var context = Substitute.For<IContext>();
            var settings = new CheckoutSettings
            {
                Customer = new CustomerSettings
                {
                    Identity = "customer@domain.com",
                    Phone = "123456789",
                    Name = "John",
                    Surname = "Doe",
                    Email = "john.doe@domain.com",
                    Document = "123456789",
                    DocumentType = "CPF"
                },
                Products = new[]
                {
                       new ProductSettings { Description = "Product 1", Price = 10.0m, Quantity = 1 },
                       new ProductSettings { Description = "Product 2", Price = 20.0m, Quantity = 2 }
                   },
                Channel = "WhatsApp",
                Currency = "BRL",
                Source = "TestSource",
                Message = "Mensagem de Teste",
                Merchant = "1234"
            };

            var cancellationToken = CancellationToken.None;
            var checkoutLink = "https://teste-media.msging.net";

            _checkoutExtension.CreateCheckOutLinkAsync(Arg.Is(settings.Merchant), Arg.Any<CheckoutDocument>(), cancellationToken)
                .Returns(checkoutLink);

            var node = new Node("teste", "wa.gw.msging.net", "#instance");

            var message = new Message()
            {
                From = new Node("teste", "wa.gw.msging.net", "#instance"),
                To = new Node("mybot", "msging.net", null),
                Content = new PlainText { Text = "Hello BLiP" }
            };

            var input = new LazyInput(message,
                message.From,
                _builderConfiguration,
                _documentSerializer,
                _envelopeSerializer,
                _artificialIntelligenceExtension,
                cancellationToken);

            context.Input.Returns(input);

            // Act  
            await _checkoutAction.ExecuteAsync(context, settings, cancellationToken);

            // Assert  
            await _checkoutExtension
                .Received(1)
                .CreateCheckOutLinkAsync(Arg.Any<string>(), Arg.Any<CheckoutDocument>(), cancellationToken);

            await _sender
                .Received(1)
                .SendMessageAsync(Arg.Is<Message>(m =>
                    m.Content.GetMediaType().Equals(MediaLink.MediaType)), cancellationToken);
        }
    }
}