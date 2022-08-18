using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a service for managing the current flow session for a given user.
    /// </summary>
    public interface IFlowSessionManager
    {
        /// <summary>
        /// Gets the current flow session for the user in the flow.
        /// </summary>
        Task<string> GetFlowSessionAsync(IContext context, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the current flow session for the user in the flow.
        /// </summary>
        Task SetFlowSessionAsync(IContext context, string flowSession, CancellationToken cancellationToken);
    }
}
