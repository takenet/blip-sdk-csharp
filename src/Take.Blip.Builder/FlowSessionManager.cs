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
            return context.GetContextVariableAsync(CURRENT_FLOW_SESSION_KEY, cancellationToken);
        }

        /// <summary>
        /// Gets the current flow session for the user in the flow.
        /// </summary>
        public Task SetFlowSessionAsync(IContext context, string flowSession, CancellationToken cancellationToken)
        {
            return context.SetVariableAsync(CURRENT_FLOW_SESSION_KEY, flowSession, cancellationToken);
        }
    }
}
