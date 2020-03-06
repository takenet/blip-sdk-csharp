using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    public interface IInputExpirationHandler
    {
        Message ValidateMessage(Message message);
        bool IsValidateState(State state, Message message);
        Task OnFlowPreProcessingAsync(State state, Message message, Node from, CancellationToken cancellationToken);
        Task OnFlowProcessedAsync(State state, Message message, Node from,  CancellationToken cancellationToken);
    }
}