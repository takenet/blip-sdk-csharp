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
        Task<string> GetStateIdAsync(IContext context, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the current state for the user in the parent flow.
        /// </summary>
        public Task<string> GetParentStateIdAsync(IContext context, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the previous state for the user in the flow.
        /// </summary>
        Task<string> GetPreviousStateIdAsync(IContext context, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the current state for the user in the flow.
        /// </summary>
        /// <returns></returns>
        Task SetStateIdAsync(IContext context, string stateId, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the previous state id for the user in the flow.
        /// This action is only informative and do not affect the user navigation.
        /// </summary>
        /// <returns></returns>
        Task SetPreviousStateIdAsync(IContext context, string previousStateId, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the current state for the user in the flow.
        /// </summary>
        Task DeleteStateIdAsync(IContext context, CancellationToken cancellationToken);
    }
}
