using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Take.Blip.Builder.Variables
{
    public class StateVariableProvider : IVariableProvider
    {
        private readonly IStateManager _stateManager;

        public StateVariableProvider(IStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public VariableSource Source => VariableSource.State;

        public async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
            var names = name.ToLowerInvariant().Split('.').ToList();

            // Determine the state
            string stateId;
            if (names.Count > 1)
            {
                switch (names[0])
                {
                    case "previous":
                        stateId = await _stateManager.GetPreviousStateIdAsync(context.Flow.Id, context.User, cancellationToken);
                        break;
                    case "current":
                        stateId = await _stateManager.GetStateIdAsync(context.Flow.Id, context.User, cancellationToken);
                        break;

                    default:
                        return null;
                }

                names.Remove(names[0]);
            }
            else
            {
                stateId = await _stateManager.GetStateIdAsync(context.Flow.Id, context.User, cancellationToken);
            }

            var state = context.Flow.States.FirstOrDefault(s => s.Id.Equals(stateId));

            if (state == null) return null;

            var variableName = names[0];

            // Determine the state property
            if (variableName == "id")  return state.Id;
            if (state.ExtensionData != null &&
                state.ExtensionData.TryGetValue(variableName, out var value))
            {
                return value.ToString(Formatting.None);
            }

            return null;
        }
    }
}
