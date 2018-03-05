using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder
{
    public class StateManager : IStateManager
    {
        private const string PREVIOUS_STATE_PREFIX = "previous";
        private const string STATE_ID_KEY = "stateId";

        private readonly IContextExtension _contextExtension;

        public StateManager(IContextExtension contextExtension)
        {
            _contextExtension = contextExtension;
        }

        public async Task<string> GetStateIdAsync(string flowId, Identity user, CancellationToken cancellationToken)
        {
            try
            {
                return await _contextExtension.GetTextVariableAsync(user, GetStateKey(flowId), cancellationToken);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return null;
            }
        }

        public async Task<string> GetPreviousStateIdAsync(string flowId, Identity user, CancellationToken cancellationToken)
        {
            try
            {
                return await _contextExtension.GetTextVariableAsync(user, GetPreviousStateKey(flowId), cancellationToken);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return null;
            }
        }

        public Task SetStateIdAsync(string flowId, Identity user, string stateId, CancellationToken cancellationToken) 
            => _contextExtension.SetTextVariableAsync(user, GetStateKey(flowId), stateId, cancellationToken);

        public Task SetPreviousStateIdAsync(string flowId, Identity user, string previousStateId, CancellationToken cancellationToken) 
            => _contextExtension.SetTextVariableAsync(user, GetPreviousStateKey(flowId), previousStateId, cancellationToken);

        public async Task DeleteStateIdAsync(string flowId, Identity user, CancellationToken cancellationToken)
        {
            try
            {
                await _contextExtension.DeleteVariableAsync(user, GetStateKey(flowId), cancellationToken);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return;
            }
        }

        private static string GetStateKey(string flowId) => $"{STATE_ID_KEY}@{flowId}";

        private static string GetPreviousStateKey(string flowId) => $"{PREVIOUS_STATE_PREFIX}-{STATE_ID_KEY}@{flowId}";
    }
}