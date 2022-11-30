using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.ForwardMessageToDesk;

namespace Take.Blip.Builder
{
    internal class LeavingFromDesk : ILeavingFromDesk
    {
        Task ILeavingFromDesk.CloseOpenedTicketsAsync(IContext context, LeavingFromDeskSettings settings, CancellationToken cancellationToken) 
            => throw new NotImplementedException();
    }
}
