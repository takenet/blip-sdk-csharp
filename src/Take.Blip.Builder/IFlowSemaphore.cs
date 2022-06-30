using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{

    /// <summary>
    /// Defines a flow semaphore service, used to block the processing of two messages from the same contact and bot
    /// </summary>
    public interface IFlowSemaphore
    {
        /// <summary>
        /// Do semaphore lock to block the processing of two messages from the same contact and bot
        /// </summary>
        /// <param name="flow">Flow</param>
        /// <param name="message">Message</param>
        /// <param name="userIdentity">User identity</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<IAsyncDisposable> WaitAsync(Flow flow, Message message, Identity userIdentity, TimeSpan timeout, CancellationToken cancellationToken);
    }
}
