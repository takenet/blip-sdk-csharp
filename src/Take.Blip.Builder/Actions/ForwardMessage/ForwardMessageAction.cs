using System;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Actions.ForwardMessage
{
    public class ForwardMessageAction : ActionBase<ForwardToAgentSettings>
    {
        public ForwardMessageAction() 
            : base(nameof(ForwardMessage))
        {
        }

        public override Task ExecuteAsync(IContext context, ForwardToAgentSettings settings, CancellationToken cancellationToken)
        {
            

            throw new NotImplementedException();
        }
    }
}
