using System;
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

        public Task<string> GetStateIdAsync(IContext context, CancellationToken cancellationToken)
        {
            return context.GetContextVariableAsync(GetStateKey(context.Flow.Id), cancellationToken);
        }

        public Task<string> GetPreviousStateIdAsync(IContext context, CancellationToken cancellationToken)
        {
            return context.GetContextVariableAsync(GetPreviousStateKey(context.Flow.Id), cancellationToken);
        }

        public Task<string> GetParentStateIdAsync(IContext context, CancellationToken cancellationToken)
        {
            return context.GetContextVariableAsync(GetStateKey(context.Flow.Parent.Id), cancellationToken);
        }

        public Task SetStateIdAsync(IContext context, string stateId, CancellationToken cancellationToken)
        {
            var expiration = context.Flow?.BuilderConfiguration?.StateExpiration ?? default;            
            return context.SetVariableAsync(GetStateKey(context.Flow.Id), stateId, cancellationToken, expiration);
        }

        public Task SetPreviousStateIdAsync(IContext context, string previousStateId, CancellationToken cancellationToken)
        {
            return context.SetVariableAsync(GetPreviousStateKey(context.Flow.Id), previousStateId, cancellationToken);
        }

        public  Task DeleteStateIdAsync(IContext context, CancellationToken cancellationToken)
        {
            return context.DeleteVariableAsync(GetStateKey(context.Flow.Id), cancellationToken);
        }

        private static string GetStateKey(string flowId) => $"{STATE_ID_KEY}@{flowId}";

        private static string GetPreviousStateKey(string flowId) => $"{PREVIOUS_STATE_PREFIX}-{STATE_ID_KEY}@{flowId}";
    }
}