using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Actions.DeleteVariable
{
    public class DeleteVariableAction : ActionBase<DeleteVariableSettings>
    {
        public DeleteVariableAction() 
            : base(nameof(DeleteVariable))
        {
        }

        public override Task ExecuteAsync(IContext context, DeleteVariableSettings settings, CancellationToken cancellationToken)
        {
            return context.DeleteVariableAsync(settings.Variable, cancellationToken);
        }
    }
}