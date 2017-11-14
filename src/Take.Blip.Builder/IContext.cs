using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    public interface IContext
    {
        string User { get; }

        Task SetVariableAsync<T>(string name, T value, CancellationToken cancellationToken);

        Task<T> GetVariableAsync<T>(string name, CancellationToken cancellationToken);

        Task<string> GetStateIdAsync(CancellationToken cancellationToken);

        Task SetStateIdAsync(string stateId, CancellationToken cancellationToken);

        Task<string> GetActionIdAsync(CancellationToken cancellationToken);

        Task SetActionIdAsync(string actionId, CancellationToken cancellationToken);
    }
}
