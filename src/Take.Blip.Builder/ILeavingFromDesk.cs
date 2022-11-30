using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.ForwardMessageToDesk;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a Leaving from Desk loader service
    /// </summary>
    public interface ILeavingFromDesk
    {
        Task CloseOpenedTicketsAsync(IContext context, LeavingFromDeskSettings settings, CancellationToken cancellationToken);
    }
}
