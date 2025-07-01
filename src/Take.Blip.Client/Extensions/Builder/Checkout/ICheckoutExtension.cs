using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Iris.Messaging.Resources.Checkout;

namespace Take.Blip.Client.Extensions.Builder.Checkout
{
    public interface ICheckoutExtension
    {
        Task<Document> CreateCheckOutLinkAsync(string merchant, CheckoutDocument document, CancellationToken cancellationToken);
    }
}
