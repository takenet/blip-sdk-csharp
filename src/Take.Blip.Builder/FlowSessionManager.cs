using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a service for managing the current flow session for a given user.
    /// </summary>
    public class FlowSessionManager : IFlowSessionManager
    {
        private const string CURRENT_FLOW_SESSION_KEY = "currentFlowSession";

        /// <summary>
        /// Gets the current flow session for the user in the flow.
        /// </summary>
        public Task<string> GetFlowSessionAsync(IContext context, CancellationToken cancellationToken)
        {
            var flowSessionId = GetStateKey(GetFlowId(context));

            return context.GetContextVariableAsync(flowSessionId, cancellationToken);
        }

        /// <summary>
        /// Sets the current flow session for the user in the flow.
        /// </summary>
        public Task SetFlowSessionAsync(IContext context, string flowSession, CancellationToken cancellationToken)
        {
            var flowSessionId = GetStateKey(GetFlowId(context));
            var expiration = context.Flow?.BuilderConfiguration?.StateExpiration ?? default;

            return context.SetVariableAsync(flowSessionId, flowSession, cancellationToken, expiration);
        }

        /// <summary>
        /// Renews the expiration of the current flow session for the user in the flow, without changing its value.
        /// </summary>
        public async Task RenewFlowSessionExpirationAsync(IContext context, CancellationToken cancellationToken)
        {
            var flowSession = await GetFlowSessionAsync(context, cancellationToken);

            if (string.IsNullOrEmpty(flowSession))
            {
                return;
            }

            await SetFlowSessionAsync(context, flowSession, cancellationToken);
        }

        private static string GetFlowId(IContext context) => context.Flow.Type == Models.FlowType.Flow ? context.Flow.Id : context.Flow.Parent?.Id;

        private static string GetStateKey(string flowId) => $"{CURRENT_FLOW_SESSION_KEY}@{flowId}";
    }
}
