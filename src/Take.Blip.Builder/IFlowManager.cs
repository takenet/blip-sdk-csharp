using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a flow management service.
    /// </summary>
    public interface IFlowManager
    {
        /// <summary>
        /// Start a new execution of the flow if the execution status is <see cref="ExecutionStatus.Stopped"/> or <see cref="ExecutionStatus.Failed"/>.
        /// If the execution is <see cref="ExecutionStatus.Suspended"/>, resume the execution. 
        /// And if it is already <see cref="ExecutionStatus.Executing"/>, silently returns.
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> TryExecuteAsync(Flow flow, string user, CancellationToken cancellationToken);
    }
}