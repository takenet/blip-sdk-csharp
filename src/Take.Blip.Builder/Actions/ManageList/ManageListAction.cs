using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Take.Blip.Client.Extensions.Broadcast;

namespace Take.Blip.Builder.Actions.ManageList
{
    public class ManageListAction : ActionBase<ManageListSettings>
    {
        private readonly IBroadcastExtension _broadcastExtension;

        public ManageListAction(IBroadcastExtension broadcastExtension)
            :base(nameof(ManageList))
        {
            _broadcastExtension = broadcastExtension;
        }

        public override async Task ExecuteAsync(IContext context, ManageListSettings settings, CancellationToken cancellationToken)
        {
            switch (settings.Action)
            {
                case ManageListSettingsAction.Add:
                    await AddToListAsync(context, settings.ListName, cancellationToken);
                    break;

                case ManageListSettingsAction.Remove:
                    await RemoveFromListAsync(context, settings.ListName, cancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task AddToListAsync(IContext context, string listName, CancellationToken cancellationToken)
        {
            var tryCount = 2;
            while (tryCount-- > 0)
            {
                try
                {
                    await _broadcastExtension.AddRecipientAsync(listName, context.User, cancellationToken);

                    return;
                }
                catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.APPLICATION_ERROR)
                {
                    // Try to create the list
                    await _broadcastExtension.CreateDistributionListAsync(listName, cancellationToken);
                }
            }

            throw new Exception("Could not add the user to the specified list");
        }

        private async Task RemoveFromListAsync(IContext context, string listName, CancellationToken cancellationToken)
        {
            try
            {
                await _broadcastExtension.DeleteRecipientAsync(listName, context.User, cancellationToken);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.APPLICATION_ERROR || 
                                           ex.Reason.Code == ReasonCodes.COMMAND_INVALID_ARGUMENT)
            {
                // Ignores if the list doesn't exists or the recipient is not member of the list
            }
        }
    }
}
