using System.Threading;
using System.Threading.Tasks;
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
        /// <param name="input">The input document.</param>
        /// <param name="user">The identity of the user that sent the input.</param>
        /// <param name="application">The identity of the application that is processing the input.</param>
        /// <param name="flow">The builder flow.</param>
        /// <param name="cancellationToken">The operation cancellation token.</param>
        /// <returns></returns>
        Task ProcessInputAsync(Document input, Identity user, Identity application, Flow flow, CancellationToken cancellationToken);
    }
}