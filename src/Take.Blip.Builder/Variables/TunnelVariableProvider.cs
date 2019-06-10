using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client.Extensions.Tunnel;

namespace Take.Blip.Builder.Variables
{
    public class TunnelVariableProvider : IVariableProvider
    {
        public VariableSource Source => VariableSource.Tunnel;
        
        public Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
            var message = context.Input.Message;
            if (message == null ||
                !message.TryGetTunnelInformation(out var tunnelInformation))
            {
                return Task.FromResult<string>(null);
            }
            
            return GetVariable(name, tunnelInformation).AsCompletedTask();
        }

        private static string GetVariable(string name, TunnelInformation tunnelInformation)
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