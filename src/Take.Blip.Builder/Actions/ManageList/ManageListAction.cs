using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Newtonsoft.Json.Linq;
using Blip.Ai.Bot.Monitoring.Logging.Abstractions;
using Blip.Ai.Bot.Monitoring.Logging.Abstractions.Models;
using Take.Blip.Client.Extensions.Broadcast;

namespace Take.Blip.Builder.Actions.ManageList
{
    public class ManageListAction : ActionBase<ManageListSettings>
    {
        private readonly IBroadcastExtension _broadcastExtension;
        private readonly IBlipLogger _blipMonitoringLogger;

        public ManageListAction(IBroadcastExtension broadcastExtension, IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(ManageList))
        {
            _broadcastExtension = broadcastExtension;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(IContext context, ManageListSettings settings, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            try
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

                this.LogExecution(_blipMonitoringLogger, context, new JObject
                {
                    ["listName"] = settings.ListName,
                    ["listAction"] = settings.Action.ToString(),
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                });
            }
            catch (Exception ex)
            {
                this.LogError(_blipMonitoringLogger, context, new JObject
                {
                    ["listName"] = settings.ListName,
                    ["listAction"] = settings.Action.ToString(),
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }, ex);
                throw;
            }
        }

        private async Task AddToListAsync(IContext context, string listName, CancellationToken cancellationToken)
        {
            var tryCount = 2;
            while (tryCount-- > 0)
            {
                try
                {
                    await _broadcastExtension.AddRecipientAsync(listName, context.UserIdentity, cancellationToken);

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
                await _broadcastExtension.DeleteRecipientAsync(listName, context.UserIdentity, cancellationToken);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.APPLICATION_ERROR || 
                                           ex.Reason.Code == ReasonCodes.COMMAND_INVALID_ARGUMENT)
            {
                // Ignores if the list doesn't exists or the recipient is not member of the list
            }
        }
    }
}
