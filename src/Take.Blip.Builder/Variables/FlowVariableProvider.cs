using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder.Variables
{
    public class FlowVariableProvider : IVariableProvider
    {
        public VariableSource Source => VariableSource.Flow;

        public async Task<string> GetVariableAsync(string name, Identity user, CancellationToken cancellationToken)
        {
            var flow = RequestContext.Flow?.Variables;
            if (flow == null) return null;
            flow.TryGetValue(name, out var value);
            return value;
        }
    }
}
