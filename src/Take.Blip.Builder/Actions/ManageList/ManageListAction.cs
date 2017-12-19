using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Newtonsoft.Json.Linq;
using Take.Blip.Client.Extensions.Broadcast;

namespace Take.Blip.Builder.Actions.ManageList
{
    public class ManageListAction : IAction
    {
        private readonly IBroadcastExtension _broadcastExtension;

        public ManageListAction(IBroadcastExtension broadcastExtension)
        {
            _broadcastExtension = broadcastExtension;
        }

        public string Type => nameof(ManageList);

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var managerListSettings = settings.ToObject<ManageListSettings>();
            if (string.IsNullOrEmpty(managerListSettings.ListName))
            {
                throw new ArgumentException($"The '{nameof(ManageListSettings.ListName)}' settings value is required for '{nameof(ManageListAction)}' action");
            }

            switch (managerListSettings.Action)
            {
                case ManageListSettingsAction.Add:
                    await AddToListAsync(context, managerListSettings.ListName, cancellationToken);
                    break;

                case ManageListSettingsAction.Remove:
                    await RemoveFromListAsync(context, managerListSettings.ListName, cancellationToken);
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
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.APPLICATION_ERROR)
            {
                // Ignores if the list doesn't exists
            }
            
        }
    }
}
