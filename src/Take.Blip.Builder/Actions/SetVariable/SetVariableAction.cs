using System;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Actions.SetVariable
{
    public class SetVariableAction : ActionBase<SetVariableSettings>
    {
        private static readonly string[] OUTPUT_PARAMETERS_NAME = new string[] { nameof(SetVariableSettings.Variable) };

        public SetVariableAction()
            : base(nameof(SetVariable), OUTPUT_PARAMETERS_NAME)
        {
            
        }

        public override Task ExecuteAsync(IContext context, SetVariableSettings settings, CancellationToken cancellationToken)
        {
            var expiration = settings.Expiration.HasValue
                ? TimeSpan.FromSeconds(settings.Expiration.Value)
                : default(TimeSpan);

            return context.SetVariableAsync(settings.Variable, settings.Value, cancellationToken, expiration);
        }
    }
}
