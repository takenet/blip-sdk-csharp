using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Serilog;
using Take.Blip.Builder.Actions.Checkout.Settings;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.Builder.Checkout;
using Take.Blip.Client.Extensions.Builder.Checkout.Documents;

namespace Take.Blip.Builder.Actions.Checkout
{
    public class CheckoutAction : ActionBase<CheckoutSettings>
    {
        private readonly ICheckoutExtension _checkoutExtension;
        private readonly ISender _sender;
        private readonly ILogger _logger;

        public CheckoutAction(ICheckoutExtension checkoutExtension, ISender sender, ILogger logger) : base(nameof(Checkout))
        {
            _checkoutExtension = checkoutExtension;
            _sender = sender;
            _logger = logger;
        }

        public override async Task ExecuteAsync(IContext context, CheckoutSettings settings, CancellationToken cancellationToken)
        {
            var checkout = new CheckoutDocument
            {
                Customer = new CustomerCheckout
                {
                    Identity = settings.Customer.Identity,
                    Phone = settings.Customer.Phone,
                    Name = settings.Customer.Name,
                    Surname = settings.Customer.Surname,
                    Email = settings.Customer.Email,
                    Document = settings.Customer.Document,
                    DocumentType = settings.Customer.DocumentType
                },
                Products = new List<ProductCheckout>(),
                Channel = settings.Channel,
                Currency = settings.Currency,
                Source = settings.Source,
            };

            foreach (var product in settings.Products)
            {
                checkout.Products.Add(new ProductCheckout
                {
                    Description = product.Description,
                    Price = product.Price,
                    Quantity = product.Quantity,
                });
            }

            var checkoutLink = await _checkoutExtension.CreateCheckOutLinkAsync(checkout, cancellationToken);

            await CreateMessageAsync(context, checkoutLink.ToString(), cancellationToken);
        }

        private async Task CreateMessageAsync(IContext context, string checkoutLink, CancellationToken cancellationToken)
        {
            var message = new Message()
            {
                Id = EnvelopeId.NewId(),
                To = context.Input.Message.From,
                Content = PlainText.Parse($"Aqui está seu link de pagamento: {checkoutLink}")
            };

            if (context.Input.Message.From.Domain.Equals("tunnel.msging.net"))
            {
                message.Metadata ??= new Dictionary<string, string>();

                if (context.Input.Message.Metadata.TryGetValue("#tunnel.owner", out string owner))
                {
                    message.Metadata.Add("#tunnel.owner", owner);
                }

                if (context.Input.Message.Metadata.TryGetValue("#tunnel.originator", out string originator))
                {
                    message.Metadata.Add("#tunnel.originator", originator);
                }
            }

            await _sender.SendMessageAsync(message, cancellationToken);
        }
    }
}
