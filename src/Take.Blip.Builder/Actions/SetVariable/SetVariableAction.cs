using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions.SetVariable
{
    public class SetVariableAction : IAction
    {
        public string Type => nameof(SetVariable);

        public Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var setVariableActionSettings = settings.ToObject<SetVariableActionSettings>();

            if (setVariableActionSettings.Variable == null) throw new ArgumentException($"The '{nameof(SetVariableActionSettings.Variable)}' settings value is required for '{nameof(SetVariable)}' action");            
            return context.SetVariableAsync(setVariableActionSettings.Variable, setVariableActionSettings.Value, cancellationToken);
        }
    }
}
