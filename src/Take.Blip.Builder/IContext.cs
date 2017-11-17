using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder
{
    public interface IContext
    {
        string FlowId { get; }

        Identity User { get; }

        Task SetVariableAsync(string name, string value, CancellationToken cancellationToken);

        Task<string> GetVariableAsync(string name, CancellationToken cancellationToken);
    }
}
