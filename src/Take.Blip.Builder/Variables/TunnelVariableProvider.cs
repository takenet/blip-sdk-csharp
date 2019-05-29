using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Variables
{
    public class TunnelVariableProvider : IVariableProvider
    {
        public VariableSource Source => VariableSource.Tunnel;
        
        public Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
            var message = context.Input.Message;
            if (message == null) return Task.FromResult<string>(null);
            
            var value = message.Metadata?.GetValueOrDefault($"#tunnel.{name}");
            return Task.FromResult(value);
        }
    }
}