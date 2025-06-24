using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client.Extensions.Builder.Checkout.Documents;

namespace Take.Blip.Client.Extensions.Builder.Checkout
{
    public class CheckoutExtension : ExtensionBase, ICheckoutExtension
    {
        private static readonly Node BuilderAddress = Node.Parse($"postmaster@builder.{Constants.DEFAULT_DOMAIN}");

        public CheckoutExtension(ISender sender) : base(sender)
        {
        }

        public async Task<Document> CreateCheckOutLinkAsync(CheckoutDocument document, CancellationToken cancellationToken)
        {
            //TODO: Insert the LimeUri
            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = BuilderAddress,
                Method = CommandMethod.Set,
                Uri = new LimeUri(""),
                Resource = document
            };

            var response = await ProcessCommandAsync<Document>(requestCommand, cancellationToken);

            return response;
        }
    }
}
