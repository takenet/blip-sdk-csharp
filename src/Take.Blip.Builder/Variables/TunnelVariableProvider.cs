using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Utils;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.Tunnel;

namespace Take.Blip.Builder.Variables
{
    public class TunnelVariableProvider : IVariableProvider
    {
        public VariableSource Source => VariableSource.Tunnel;
        
        public Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
            var message = context.Input.Message;
            if (message == null) return Task.FromResult<string>(null);
            
            var value = message.Metadata?.GetValueOrDefault($"{TunnelExtension.TUNNEL_METADATA_KEY_PREFIX}.{name}");
            return Task.FromResult(value);
        }
    }
}