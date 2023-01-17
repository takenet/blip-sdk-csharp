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

        private const int STATE_ID_INDEX = 0;
        private const int PREVIOUS_STATE_ID_INDEX = 1;
        private const string STATE_AND_PREVIOUS_SEPARATOR = ";";

        public async Task<string> GetStateIdAsync(IContext context, CancellationToken cancellationToken) => ExtractStateId(await GetStateIdContextVariable(context, cancellationToken));

        public async Task<string> GetPreviousStateIdAsync(IContext context, CancellationToken cancellationToken)
        {
            var previousStateId = ExtractPreviousStateId(await GetStateIdContextVariable(context, cancellationToken));

            //using GetPreviousStateKey to compatibility with previous versions
            return previousStateId.IsNullOrEmpty() ? await context.GetContextVariableAsync(GetPreviousStateKey(context.Flow.Id), cancellationToken) : previousStateId;
        }

        public async Task<string> GetParentStateIdAsync(IContext context, CancellationToken cancellationToken)
        {
            if (context.Flow.Parent == null)
            {
                return null;
            }

            return ExtractStateId(await GetParentStateIdContextVariable(context, cancellationToken));
        }

        public Task SetStateIdAsync(IContext context, string stateId, string previousStateId, CancellationToken cancellationToken)
        {
            var expiration = context.Flow?.BuilderConfiguration?.StateExpiration ?? default;            
            return context.SetVariableAsync(GetStateKey(context.Flow.Id), $"{stateId}{STATE_AND_PREVIOUS_SEPARATOR}{previousStateId}", cancellationToken, expiration);
        }

        public  Task DeleteStateIdAsync(IContext context, CancellationToken cancellationToken) => context.DeleteVariableAsync(GetStateKey(context.Flow.Id), cancellationToken);

        private static string GetStateKey(string flowId) => $"{STATE_ID_KEY}@{flowId}";

        private static string GetPreviousStateKey(string flowId) => $"{PREVIOUS_STATE_PREFIX}-{STATE_ID_KEY}@{flowId}";

        private static string ExtractStateId(string value) => value.IsNullOrEmpty() || !value.Contains(STATE_AND_PREVIOUS_SEPARATOR) ? value : value.Split(STATE_AND_PREVIOUS_SEPARATOR)[STATE_ID_INDEX];

        private static string ExtractPreviousStateId(string value)
        {
            if (value.IsNullOrEmpty())
            {
                return value;
            }

            if (value.Contains(STATE_AND_PREVIOUS_SEPARATOR))
            {
                return value.Split(STATE_AND_PREVIOUS_SEPARATOR)[PREVIOUS_STATE_ID_INDEX];
            }

            return "";
        }

        private Task<string> GetStateIdContextVariable(IContext context, CancellationToken cancellationToken) => context.GetContextVariableAsync(GetStateKey(context.Flow.Id), cancellationToken);
        private Task<string> GetParentStateIdContextVariable(IContext context, CancellationToken cancellationToken) => context.GetContextVariableAsync(GetStateKey(context.Flow.Parent.Id), cancellationToken);
    }
}