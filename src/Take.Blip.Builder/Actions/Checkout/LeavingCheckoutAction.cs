using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Actions.Checkout.Settings;

namespace Take.Blip.Builder.Actions.Checkout
{
    /// <summary>
    /// Action that handles the leaving of a checkout process.
    /// </summary>
    public class LeavingCheckoutAction : ActionBase<LeavingCheckoutSettings>
    {
        private readonly ILogger _logger;

        private const string LEAVING_CHECKOUT_ACTION_NAME = "LeavingCheckout";

        public LeavingCheckoutAction(ILogger logger): base(LEAVING_CHECKOUT_ACTION_NAME)
        {
            _logger = logger;
        }

        public override async Task ExecuteAsync(IContext context, LeavingCheckoutSettings settings, CancellationToken cancellationToken)
        {
            _logger.Information("{Area} Leaving checkout action executed for user {UserIdentity}.", nameof(LeavingCheckoutAction), context.UserIdentity);

            await Task.WhenAll(new Task[] {
                context.DeleteCheckoutStateAsync(cancellationToken),
                context.DeleteCheckoutResponseStateAsync(cancellationToken),
            });
        }
    }
}
