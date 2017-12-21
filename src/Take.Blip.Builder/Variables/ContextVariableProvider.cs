using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder.Variables
{
    public class ContextVariableProvider : IVariableProvider
    {
        private readonly IContextExtension _contextExtension;

        public ContextVariableProvider(IContextExtension contextExtension)
        {
            _contextExtension = contextExtension;
        }

        public VariableSource Source => VariableSource.Context;

        public async Task<string> GetVariableAsync(string name, Identity user, CancellationToken cancellationToken)
        {
            try
            {
                return await _contextExtension.GetTextVariableAsync(user, name, cancellationToken);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return null;
            }
        }
    }
}
