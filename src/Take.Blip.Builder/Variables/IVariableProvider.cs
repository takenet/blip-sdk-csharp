using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder.Variables
{
    public interface IVariableProvider
    {
        VariableSource Source { get; }

        Task<string> GetVariableAsync(string name, Identity user, CancellationToken cancellationToken);
    }
}
