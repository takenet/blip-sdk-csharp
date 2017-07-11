using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Receivers
{
    /// <summary>
    /// Notification receiver that simply ignores the received envelope.
    /// </summary>
    public class BlackholeEnvelopeReceiver : IEnvelopeReceiver<Envelope>
    {
        public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;
    }
}