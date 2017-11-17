using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder
{
    public interface IStorageManager
    {
        Task<string> GetStateIdAsync(string flowId, Identity user, CancellationToken cancellationToken);

        Task SetStateIdAsync(string flowId, Identity user, string stateId, CancellationToken cancellationToken);

        Task DeleteStateIdAsync(string flowId, Identity user, CancellationToken cancellationToken);
    }
}
