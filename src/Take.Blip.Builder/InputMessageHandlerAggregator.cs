using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    public class InputMessageHandlerAggregator : IInputMessageHandlerAggregator
    {
        private readonly IEnumerable<IInputMessageHandler> _handlers;

        public InputMessageHandlerAggregator(IEnumerable<IInputMessageHandler> handlers)
        {
            _handlers = handlers;
        }

        public IEnumerable<IInputMessageHandler> GetHandlers() => _handlers;

        public (bool MessageHasChanged, Message NewMessage) HandleMessage(Message message)
        {
            foreach (var handler in _handlers)
            {
                var (messageHasChanged, newMessage) = handler.HandleMessage(message);
                if (messageHasChanged)
                {
                    return (messageHasChanged, newMessage);
                }
            }

            return (false, message);
        }

        public bool IsValidateState(State state, Message message)
        {
            foreach (var handler in _handlers)
            {
                if (!handler.IsValidateState(state, message))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task OnFlowPreProcessingAsync(State state, Message message, Node from, CancellationToken cancellationToken) =>
            _handlers.ForEach(handler => handler.OnFlowPreProcessingAsync(state, message, from, cancellationToken));

        public async Task OnFlowProcessedAsync(State state, Message message, Node from, CancellationToken cancellationToken) =>
            _handlers.ForEach(handler => handler.OnFlowProcessedAsync(state, message, from, cancellationToken));
    }
}
