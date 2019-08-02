using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Actions.CreateTicket
{
    public class CreateTicketAction : ActionBase<CreateTicketSettings>
    {
        public CreateTicketAction(string type) : base(type)
        {
        }

        public override Task ExecuteAsync(IContext context, CreateTicketSettings settings, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}