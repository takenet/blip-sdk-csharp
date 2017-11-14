using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    public interface IContext
    {
        string User { get; }

        Task SetVariableAsync(string name, string value, CancellationToken cancellationToken);

        Task<string> GetVariableAsync(string name, CancellationToken cancellationToken);

        Task<string> GetStateIdAsync(CancellationToken cancellationToken);

        Task SetStateIdAsync(string stateId, CancellationToken cancellationToken);

        Task<string> GetActionIdAsync(CancellationToken cancellationToken);

        Task SetActionIdAsync(string actionId, CancellationToken cancellationToken);
    }
}
