using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Client.Extensions.Tunnel;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder.Variables
{
    public class TunnelVariableProvider : IVariableProvider
    {
        private readonly ITunnelExtension _tunnelExtension;

        public TunnelVariableProvider(ITunnelExtension tunnelExtension)
        {
            _tunnelExtension = tunnelExtension;
        }
        
        public VariableSource Source => VariableSource.Tunnel;
        
        public async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
            var tunnel = await _tunnelExtension.TryGetTunnelForMessageAsync(context.Input.Message, cancellationToken);
            if (tunnel == null) return null;
            
            return GetVariable(name, tunnel);
        }

        private static string GetVariable(string name, Tunnel tunnelInformation)
        {
            switch (name)
            {
                case "owner":
                    return tunnelInformation.Owner;
                
                case "originator":
                    return tunnelInformation.Originator;
                
                case "destination":
                    return tunnelInformation.Destination;
                
                default:
                    return null;
            }
        }
    }
}