using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder
{
    public class Context : IContext
    {
        private readonly IContextExtension _contextExtension;        

        public Context(IContextExtension contextExtension, string flowId, Identity user)
        {
            _contextExtension = contextExtension;
            User = user ?? throw new ArgumentNullException(nameof(user));
            FlowId = flowId;
        }

        public string FlowId { get; set; }

        public Identity User { get; }

        public Task SetVariableAsync(string name, string value, CancellationToken cancellationToken) 
            => _contextExtension.SetTextVariableAsync(User, name, value, cancellationToken);

        public async Task<string> GetVariableAsync(string name, CancellationToken cancellationToken)
        {
            try
            {
                return await _contextExtension.GetTextVariableAsync(User, name, cancellationToken);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return null;
            }
        }
    }
}