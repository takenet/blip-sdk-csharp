using System;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Actions.SetVariable
{
    public class SetVariableAction : ActionBase<SetVariableSettings>
    {
        public SetVariableAction()
            : base(nameof(SetVariable))
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
