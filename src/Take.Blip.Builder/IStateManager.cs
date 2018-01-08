using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a service for managing the active state for a given user.
    /// </summary>
    public interface IStateManager
    {
        /// <summary>
        /// Gets the current state for the user in the flow.
        /// </summary>
        /// <param name="flowId"></param>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> GetStateIdAsync(string flowId, Identity user, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the state for the user in the flow.
        /// </summary>
        /// <param name="flowId"></param>
        /// <param name="user"></param>
        /// <param name="stateId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetStateIdAsync(string flowId, Identity user, string stateId, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the current state for the user in the flow.
        /// </summary>
        /// <param name="flowId"></param>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteStateIdAsync(string flowId, Identity user, CancellationToken cancellationToken);
    }
}
