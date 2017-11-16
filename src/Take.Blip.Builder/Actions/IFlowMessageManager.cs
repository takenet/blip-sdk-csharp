using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder.Actions
{
    public interface IFlowMessageManager
    {
        Task EnqueueMessageAsync(Flow flow, string user, Message message, CancellationToken cancellationToken);

        Task<Message> DequeueMessageAsync(string user, CancellationToken cancellationToken);
    }
}