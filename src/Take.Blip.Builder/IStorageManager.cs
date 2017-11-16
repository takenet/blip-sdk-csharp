using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    public interface IStorageManager
    {
        Task<ExecutionStatus> GetExecutionStatusAsync(string flowId, string user, CancellationToken cancellationToken);

        Task SetExecutionStatusAsync(string flowId, string user, ExecutionStatus executionStatus, CancellationToken cancellationToken);

        Task<string> GetStateIdAsync(string flowId, string user, CancellationToken cancellationToken);

        Task SetStateIdAsync(string flowId, string user, string stateId, CancellationToken cancellationToken);

        Task DeleteStateIdAsync(string flowId, string user, CancellationToken cancellationToken);

        Task<string> GetActionIdAsync(string flowId, string user, CancellationToken cancellationToken);

        Task SetActionIdAsync(string flowId, string user, string actionId, CancellationToken cancellationToken);

        Task DeleteActionIdAsync(string flowId, string user, CancellationToken cancellationToken);
    }
}
