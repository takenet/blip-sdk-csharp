using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a flow management service.
    /// </summary>
    public interface IFlowManager
    {
        /// <summary>
        /// Process a given input.
        /// </summary>
        /// <param name="message">The input message.</param>
        /// <param name="flow">The builder flow.</param>
        /// <param name="contact">The contact for the message sender, if available.</param>
        /// <param name="cancellationToken">The operation cancellation token.</param>
        /// <returns></returns>
        Task ProcessInputAsync(Message message, Flow flow, Contact contact, CancellationToken cancellationToken);
    }
}