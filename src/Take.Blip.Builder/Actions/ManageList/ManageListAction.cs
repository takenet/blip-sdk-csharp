using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
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

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "ManageList",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["listName"] = settings.ListName,
                        ["listAction"] = settings.Action.ToString(),
                        ["success"] = true,
                    },
                    FlowVersion = context.Flow?.Version,
                    Channel = context.Input.Message?.From?.Domain,
                    IdMessage = context.Input.Message?.Id,
                    From = context.UserIdentity?.ToString(),
                    To = context.OwnerIdentity?.ToString(),
                    OriginalFrom = context.Input.Message?.From,
                    OriginalTo = context.Input.Message?.To,
                });
            }
            catch (Exception ex)
            {
                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "ManageList",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["listName"] = settings.ListName,
                        ["listAction"] = settings.Action.ToString(),
                        ["success"] = false,
                        ["error"] = ex.ToString(),
                    },
                    FlowVersion = context.Flow?.Version,
                    Channel = context.Input.Message?.From?.Domain,
                    IdMessage = context.Input.Message?.Id,
                    From = context.UserIdentity?.ToString(),
                    To = context.OwnerIdentity?.ToString(),
                    OriginalFrom = context.Input.Message?.From,
                    OriginalTo = context.Input.Message?.To,
                });
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
