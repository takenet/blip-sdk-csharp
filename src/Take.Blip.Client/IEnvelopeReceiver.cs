using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client
{
    public interface IEnvelopeReceiver<in TEnvelope>
        where TEnvelope : Envelope
    {
        /// <summary>
        /// Receives an <see cref="TEnvelope"/> envelope.
        /// </summary>
        /// <param name="envelope">Envelope type</param>
        /// <param name="cancellationToken">A cancellation token to allow the task to be canceled</param>
        /// <returns>Task representing the receive operation</returns>
        Task ReceiveAsync(TEnvelope envelope, CancellationToken cancellationToken = default(CancellationToken));
    }
}
