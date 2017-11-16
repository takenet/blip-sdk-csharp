using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Hosting;

namespace Take.Blip.Builder
{
    public class FlowManager : IFlowManager
    {
        private readonly IConfiguration _configuration;
        private readonly IStorageManager _storageManager;
        private readonly IContextProvider _contextProvider;
        private readonly INamedSemaphore _namedSemaphore;
        private readonly IActionProvider _actionProvider;

        public FlowManager(
            IConfiguration configuration,
            IStorageManager storageManager, 
            IContextProvider contextProvider, 
            INamedSemaphore namedSemaphore, 
            IActionProvider actionProvider)
        {
            _configuration = configuration;
            _storageManager = storageManager;
            _contextProvider = contextProvider;
            _namedSemaphore = namedSemaphore;
            _actionProvider = actionProvider;
        }

        public async Task ExecuteAsync(Flow flow, string user, CancellationToken cancellationToken)
        {
            if (flow == null) throw new ArgumentNullException(nameof(flow));
            if (user == null) throw new ArgumentNullException(nameof(user));
            flow.Validate();

            ExecutionStatus currentExecutionStatus;

            var handle = await GetSynchronizationHandleAsync(flow, user, cancellationToken);
            try
            {
                currentExecutionStatus = await _storageManager.GetExecutionStatusAsync(flow.Id, user, cancellationToken);
                // Ignores if the execution is already in progress
                if (currentExecutionStatus == ExecutionStatus.Executing) return;
                
                await _storageManager.SetExecutionStatusAsync(flow.Id, user, ExecutionStatus.Executing, cancellationToken);
            }
            finally
            {
                await handle.DisposeAsync();
            }

            try
            {
                State state = null;

                // If already executing, try retrieve the stored state
                if (currentExecutionStatus == ExecutionStatus.Suspended)
                {
                    var stateId = await _storageManager.GetStateIdAsync(flow.Id, user, cancellationToken);
                    state = flow.States.FirstOrDefault(s => s.Id == stateId);
                }

                if (state == null)
                {
                    // Use the flow root state
                    state = flow.States.Single(s => s.Root);
                }

                // Load the user context
                var context = await _contextProvider.GetContextAsync(flow.Id, user, cancellationToken);

                // While the user is in a state
                while (state != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!await ProcessActionsAsync(context, flow, state, cancellationToken))
                    {
                        // Change the execution status to Suspended for the current user
                        await SynchronizedSetExecutionStatusAsync(flow, user, ExecutionStatus.Suspended, cancellationToken);
                        return;
                    }
                    state = await ProcessOutputsAsync(context, flow, state, cancellationToken);
                }

                // Change the execution status to Stopped for the current user
                await SynchronizedSetExecutionStatusAsync(flow, user, ExecutionStatus.Stopped, cancellationToken);
            }
            catch
            {
                if (cancellationToken.IsCancellationRequested) cancellationToken = CancellationToken.None;
                await SynchronizedSetExecutionStatusAsync(flow, user, ExecutionStatus.Failed, cancellationToken);
                throw;
            }
        }

        private async Task SynchronizedSetExecutionStatusAsync(Flow flow, string user, ExecutionStatus executionStatus, CancellationToken cancellationToken)
        {
            var handle = await GetSynchronizationHandleAsync(flow, user, cancellationToken);
            try
            {
                var currentExecutionStatus = await _storageManager.GetExecutionStatusAsync(flow.Id, user, cancellationToken);
                if (!currentExecutionStatus.CanChangeTo(executionStatus))
                {
                    throw new InvalidOperationException($"Cannot change from status '${currentExecutionStatus}' to '{executionStatus}'");
                }
                await _storageManager.SetExecutionStatusAsync(flow.Id, user, executionStatus, cancellationToken);
            }
            finally
            {
                await handle.DisposeAsync();
            }
        }

        private Task<IAsyncDisposable> GetSynchronizationHandleAsync(Flow flow, string user, CancellationToken cancellationToken)
        {
            return _namedSemaphore.WaitAsync($"{flow.Id}:{user}", _configuration.ExecutionSemaphoreExpiration, cancellationToken);            
        }

        private async Task<bool> ProcessActionsAsync(IContext context, Flow flow, State state, CancellationToken cancellationToken)
        {
            var actionOrder = 0;

            var actionId = await _storageManager.GetActionIdAsync(flow.Id, context.User, cancellationToken);
            if (actionId != null)
            {
                actionOrder = state.Actions.FirstOrDefault(a => a.Id == actionId)?.Order ?? 0;
            }

            // Execute all state actions
            foreach (var stateAction in state.Actions.OrderBy(a => a.Order).Where(a => a.Order >= actionOrder))
            {                
                var action = _actionProvider.Get(stateAction.Name);

                // If the action is not able to execute, suspend the flow execution.
                if (!await action.ExecuteAsync(context, stateAction.Argument, cancellationToken))
                {
                    await _storageManager.SetActionIdAsync(flow.Id, context.User, stateAction.Id, cancellationToken);
                    return false;
                }
            }

            // Reset the action id
            await _storageManager.DeleteActionIdAsync(flow.Id, context.User, cancellationToken);
            return true;
        }

        private async Task<State> ProcessOutputsAsync(IContext context, Flow flow, State state, CancellationToken cancellationToken)
        {
            var outputs = state.Outputs;
            state = null;

            // If there's any output in the current state
            if (outputs != null)
            {
                // Evalute each output conditions
                foreach (var output in outputs.OrderBy(o => o.Order))
                {
                    var isValidOutput = true;

                    if (output.Conditions != null)
                    {
                        foreach (var outputCondition in output.Conditions)
                        {
                            isValidOutput = await EvaluateConditionAsync(outputCondition, context, cancellationToken);
                            if (!isValidOutput) break;
                        }
                    }

                    if (isValidOutput)
                    {
                        state = flow.States.SingleOrDefault(s => s.Id == output.StateId);
                        break;
                    }
                }
            }

            await _storageManager.SetStateIdAsync(flow.Id, context.User, state?.Id, cancellationToken);
            return state;
        }

        public async Task<bool> EvaluateConditionAsync(Condition condition, IContext context, CancellationToken cancellationToken)
        {
            switch (condition.Comparison)
            {
                case ConditionComparison.Equals:
                    var variableValue = await context.GetVariableAsync(condition.Variable, cancellationToken);
                    return variableValue == condition.Value;
            }

            throw new NotImplementedException();
        }
    }
}