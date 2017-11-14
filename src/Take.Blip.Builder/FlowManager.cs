using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions;

namespace Take.Blip.Builder
{
    public class FlowManager : IFlowManager
    {
        private readonly IContextProvider _contextProvider;
        private readonly IFlow _flow;
        private readonly IDistributedMutex _distributedMutex;
        private readonly IActionFactory _actionFactory;

        public FlowManager(IContextProvider contextProvider, IFlow flow, IDistributedMutex distributedMutex, IActionFactory actionFactory)
        {
            _contextProvider = contextProvider;
            _flow = flow;
            _distributedMutex = distributedMutex;
            _actionFactory = actionFactory;
        }

        public async Task ProcessAsync(string user, CancellationToken cancellationToken)
        {
            using (await _distributedMutex.WaitAsync(user, TimeSpan.FromSeconds(60), cancellationToken))
            {
                var context = await _contextProvider.GetContextAsync(user, cancellationToken);

                // Get the current state from the context or use the flow root state
                var stateId = await context.GetStateIdAsync(cancellationToken) ?? _flow.States.Single(s => s.Root).Id;

                var state = _flow.States.SingleOrDefault(s => s.Id == stateId);
                if (state == null) throw new ArgumentException($"State '{stateId}' not found");
                
                // While the user is on a state
                while (state != null)
                {
                    if (!await ProcessActionsAsync(context, state, cancellationToken)) return;
                    state = await ProcessOutputsAsync(context, state, cancellationToken);
                }
            }
        }

        private async Task<bool> ProcessActionsAsync(IContext context, State state, CancellationToken cancellationToken)
        {
            var actionOrder = 0;

            var actionId = await context.GetActionIdAsync(cancellationToken);
            if (actionId != null)
            {
                actionOrder = state.Actions.FirstOrDefault(a => a.Id == actionId)?.Order ?? 0;
            }

            // Execute all state actions
            foreach (var stateAction in state.Actions.OrderBy(a => a.Order).Where(a => a.Order >= actionOrder))
            {
                var action = GetAction(stateAction);

                // If the action is not ready to execute, suspend the execution
                if (!await action.CanExecuteAsync(context, cancellationToken))
                {
                    await context.SetActionIdAsync(stateAction.Id, cancellationToken);
                    return false;
                }

                await action.ExecuteAsync(context, cancellationToken);
            }

            // Reset the action id
            await context.SetActionIdAsync(null, cancellationToken);
            return true;
        }

        private async Task<State> ProcessOutputsAsync(IContext context, State state, CancellationToken cancellationToken)
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
                        state = _flow.States.SingleOrDefault(s => s.Id == output.StateId);
                        break;
                    }
                }
            }

            await context.SetStateIdAsync(state?.Id, cancellationToken);
            return state;
        }

        public IAction GetAction(Action action)
        {
            return _actionFactory.Create(action.Type, action.Settings);
        }

        public async Task<bool> EvaluateConditionAsync(Condition condition, IContext context, CancellationToken cancellationToken)
        {
            switch (condition.Comparison)
            {
                case ConditionComparison.Equals:
                    var variableValue = context.GetVariableAsync(condition.Variable, cancellationToken);
                    break;
            }
            throw new NotImplementedException();

        }


    }
}