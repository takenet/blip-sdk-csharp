using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.ForwardMessageToDesk;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    internal class ForwardToDesk : IForwardToDesk
    {
        Task<bool> IForwardToDesk.GetOrCreateTicketAsync(IContext context, ForwardToDeskSettings settings, CancellationToken cancellationToken) => throw new NotImplementedException();
        Task IForwardToDesk.SendMessageAsync(IContext context, ForwardToDeskSettings settings, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}