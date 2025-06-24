using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client.Extensions.Builder.Checkout.Documents;

namespace Take.Blip.Client.Extensions.Builder.Checkout
{
    public interface ICheckoutExtension
    {
        Task<Document> CreateCheckOutLinkAsync(CheckoutDocument document, CancellationToken cancellationToken);
    }
}
