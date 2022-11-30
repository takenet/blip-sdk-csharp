using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.ForwardMessageToDesk;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a Forward to Desk loader service
    /// </summary>
    public interface IForwardToDesk
    {
        Task<bool> GetOrCreateTicketAsync(IContext context, ForwardToDeskSettings settings, CancellationToken cancellationToken);

        Task SendMessageAsync(IContext context, ForwardToDeskSettings settings, CancellationToken cancellationToken);
    }
}
