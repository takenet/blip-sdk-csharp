using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources.Checkout;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Builder.Checkout
{
    public class CheckoutExtension : ExtensionBase, ICheckoutExtension
    {
        private static readonly Node BuilderAddress = Node.Parse($"postmaster@builder.{Constants.DEFAULT_DOMAIN}");
        private const string CHECKOUT_LINK_CREATION = "/checkout/link/{0}";
        public CheckoutExtension(ISender sender) : base(sender)
        {
        }

        public async Task<Document> CreateCheckOutLinkAsync(string merchant, CheckoutDocument document, CancellationToken cancellationToken)
        {
            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = BuilderAddress,
                Method = CommandMethod.Set,
                Uri = new LimeUri(string.Format(CHECKOUT_LINK_CREATION, merchant)),
                Resource = document
            };

            var response = await ProcessCommandAsync<Document>(requestCommand, cancellationToken);

            return response;
        }
    }
}
