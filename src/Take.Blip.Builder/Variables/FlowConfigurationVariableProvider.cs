using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder.Variables
{
    public class FlowConfigurationVariableProvider : IVariableProvider
    {
        public VariableSource Source => VariableSource.Config;

        public async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
            var configuration = context.Flow.Configuration;
            if (configuration == null) return null;
            configuration.TryGetValue(name, out var value);
            return value;
        }
    }
}
