using System.Threading.Tasks;
using System.Threading;
using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    public interface IInputMessageHandler
    {
        (bool, Message) HandleMessage(Message message);
        bool IsValidateState(State state, Message message);
        Task OnFlowPreProcessingAsync(State state, Message message, Node from, CancellationToken cancellationToken);
        Task OnFlowProcessedAsync(State state, Message message, Node from, CancellationToken cancellationToken);
    }
}
