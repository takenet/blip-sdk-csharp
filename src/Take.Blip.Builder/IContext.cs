using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    public interface IContext
    {
        string FlowId { get; }

        string User { get; }

        Task SetVariableAsync(string name, string value, CancellationToken cancellationToken);

        Task<string> GetVariableAsync(string name, CancellationToken cancellationToken);
    }
}
