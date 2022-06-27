using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Storage;

namespace Take.Blip.Builder
{
    class BasicFlowSemaphore: IFlowSemaphore
    {
        private readonly INamedSemaphore _namedSemaphore;

        public BasicFlowSemaphore(
            INamedSemaphore namedSemaphore
            )
        {
            _namedSemaphore = namedSemaphore;
        }

        public Task<IAsyncDisposable> WaitAsync(Flow flow, Message message, Identity userIdentity, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return _namedSemaphore.WaitAsync($"{flow.Id}:{userIdentity}", timeout, cancellationToken);
        }
    }
}
