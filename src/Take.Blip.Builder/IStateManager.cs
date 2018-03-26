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
        /// Gets the previous state for the user in the flow.
        /// </summary>
        /// <param name="flowId"></param>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> GetPreviousStateIdAsync(string flowId, Identity user, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the current state for the user in the flow.
        /// </summary>
        /// <param name="flowId"></param>
        /// <param name="user"></param>
        /// <param name="stateId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetStateIdAsync(string flowId, Identity user, string stateId, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the previous state id for the user in the flow.
        /// This action is only informative and do not affect the user navigation.
        /// </summary>
        /// <param name="flowId"></param>
        /// <param name="user"></param>
        /// <param name="previousStateId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetPreviousStateIdAsync(string flowId, Identity user, string previousStateId, CancellationToken cancellationToken);

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
