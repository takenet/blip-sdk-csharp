using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Messaging.Resources.Checkout;
using Lime.Protocol;
using Serilog;
using Take.Blip.Builder.Actions.Checkout.Settings;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.Builder.Checkout;

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
            if (await CheckIfCheckoutLinkWasAlrearyCreatedAsync(context, cancellationToken))
                // TODO: Send the default message if the link was already created and we didn´t got the response from API
                return;

            try
            {
                await GenerateLinkAsync(context, settings, cancellationToken);
                await context.SetCheckoutGeneratedLinkStatusAsync(Constants.CHECKOUT_GENERATED_LINK_SUCCESS_VALUE, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "{Area} Error when creating the checkout link.", nameof(CheckoutAction));
                await context.SetCheckoutGeneratedLinkStatusAsync(Constants.CHECKOUT_GENERATED_LINK_FAILED_VALUE, cancellationToken);
            }
        }

        private async Task<bool> CheckIfCheckoutLinkWasAlrearyCreatedAsync(IContext context, CancellationToken cancellationToken)
        {
            var checkoutGeneratedLinkStatus = await context.GetCheckoutGeneratedLinkStatusAsync(cancellationToken);

            return checkoutGeneratedLinkStatus != null && checkoutGeneratedLinkStatus.Equals(Constants.CHECKOUT_GENERATED_LINK_SUCCESS_VALUE);
        }

        private async Task GenerateLinkAsync(IContext context, CheckoutSettings settings, CancellationToken cancellationToken)
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

            var checkoutLink = await _checkoutExtension.CreateCheckOutLinkAsync(settings.Merchant, checkout, cancellationToken);

            await CreateMessageAsync(context, settings.Message, checkoutLink.ToString(), cancellationToken);

        }

        private async Task CreateMessageAsync(IContext context, string mediaText, string checkoutLink, CancellationToken cancellationToken)
        {
            //TODO: Change generated type to deliver the Component Order properly
            var message = new Message()
            {
                Id = EnvelopeId.NewId(),
                To = context.Input.Message.From,
                Content = new WebLink
                {
                    Text = mediaText,
                    Uri = new Uri(checkoutLink)
                }
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
