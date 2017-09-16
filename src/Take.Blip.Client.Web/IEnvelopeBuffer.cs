using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Web
{
    public interface IEnvelopeBuffer
    {
        Task SendAsync(Envelope envelope, CancellationToken cancellationToken);

        Task<Envelope> ReceiveAsync(CancellationToken cancellationToken);
    }
}
