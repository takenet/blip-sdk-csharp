using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder.Actions.ForwardMessageToDesk
{
    public class ForwardMessageToDeskAction : ActionBase<ForwardMessageToDeskSettings>
    {
        public ForwardMessageToDeskAction() 
            : base(nameof(ForwardMessageToDesk))
        {
        }

        public override Task ExecuteAsync(IContext context, ForwardMessageToDeskSettings settings, CancellationToken cancellationToken)
        {
            var message = new Message
            {
                Id = EnvelopeId.NewId(),
                Content = context.Input.Content
            };
            

            throw new NotImplementedException();
        }
    }
}
