using System.Threading.Tasks;
using System.Threading;
using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    public interface IInputMessageHandler
    {
        (bool MessageHasChanged, Message NewMessage) HandleMessage(Message message);
        bool IsValidateState(State state, Message message, Flow flow);
        Task OnFlowPreProcessingAsync(State state, Message message, Node from, IContext context, CancellationToken cancellationToken);
        Task OnFlowProcessedAsync(State state, Flow flow, Message message, Node from, CancellationToken cancellationToken);
    }
}
