using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Diagnostics
{
    public interface ITraceManager
    {
        (StateTrace, Stopwatch) CreateStateTrace(InputTrace inputTrace, State state, StateTrace stateTrace = null, Stopwatch stateStopwatch = null);

        Task ProcessTraceAsync(InputTrace inputTrace, TraceSettings traceSettings, Stopwatch inputStopwatch, CancellationToken cancellationToken);
    }
}