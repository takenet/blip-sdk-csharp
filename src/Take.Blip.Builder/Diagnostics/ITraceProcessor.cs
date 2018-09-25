using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Diagnostics
{
    public interface ITraceProcessor
    {
        Task ProcessTraceAsync(TraceEvent traceEvent, CancellationToken cancellationToken);
    }
}
