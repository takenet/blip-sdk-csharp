using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Variables
{
    public interface IVariableProvider
    {
        VariableSource Source { get; }

        Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken);
    }
}
