using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Actions.Checkout
{
    public static class CheckoutStatusExtensions
    {
        /// <summary>
        /// Sets the status of the checkout generated link in the context variable to indicate success.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task SetCheckoutGeneratedLinkStatusAsync(this IContext context, string value, CancellationToken cancellationToken)
        {
            await context.SetVariableAsync(Constants.CHECKOUT_GENERATED_LINK_STATUS_KEY, value, cancellationToken);
        }

        /// <summary>
        /// Gets the status of the checkout generated link from the context variable.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<string> GetCheckoutGeneratedLinkStatusAsync(this IContext context, CancellationToken cancellationToken)
        {
            var contextVariable = await context.GetVariableAsync(Constants.CHECKOUT_GENERATED_LINK_STATUS_KEY, cancellationToken);

            return contextVariable;
        }

        /// <summary>
        /// Deletes the checkout generated link status from the context variable.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task DeleteCheckoutStateAsync(this IContext context, CancellationToken cancellationToken)
        {
            await context.DeleteVariableAsync(Constants.CHECKOUT_GENERATED_LINK_STATUS_KEY, cancellationToken);
        }

        /// <summary>
        /// Deletes the response of the checkout generated link in the context variable.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task DeleteCheckoutResponseStateAsync(this IContext context, CancellationToken cancellationToken)
        {
            await context.DeleteVariableAsync(Constants.CHECKOUT_GENERATED_LINK_RESPONSE_KEY, cancellationToken);
        }
    }
}



