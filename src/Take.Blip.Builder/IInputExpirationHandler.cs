using Lime.Protocol;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    public interface IInputExpirationHandler
    {
        (bool, Message) ValidateMessage(Message message);
        bool IsValidateState(State state, Message message);
        Task OnFlowPreProcessingAsync(State state, Message message, Node from, CancellationToken cancellationToken);
        Task OnFlowProcessedAsync(State state, Message message, Node from, CancellationToken cancellationToken);
    }
}