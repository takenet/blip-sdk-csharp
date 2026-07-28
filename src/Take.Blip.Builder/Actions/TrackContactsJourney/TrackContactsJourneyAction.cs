using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
using Take.Blip.Client.Extensions.ContactsJourney;

namespace Take.Blip.Builder.Actions.TrackContactsJourney
{
    public class TrackContactsJourneyAction : ActionBase<TrackContactsJourneySettings>
    {
        private readonly IContactsJourneyExtension _contactsJourneyExtension;
        private readonly IBlipLogger _blipMonitoringLogger;

        public TrackContactsJourneyAction(IContactsJourneyExtension contactsJourneyExtension, IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(TrackContactsJourney))
        {
            _contactsJourneyExtension = contactsJourneyExtension;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(IContext context, TrackContactsJourneySettings settings, CancellationToken cancellationToken)
        {
            try
            {
                await _contactsJourneyExtension.AddAsync(
                    settings.StateId,
                    settings.StateName,
                    settings.PreviousStateId,
                    settings.PreviousStateName,
                    contactIdentity: context.UserIdentity,
                    fireAndForget: settings.FireAndForget ?? true,
                    cancellationToken: cancellationToken
                    );

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "TrackContactsJourney",
                    EventType = "ActionExecution",
                    StateId = context.GetCurrentStateId(),
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["stateId"] = settings.StateId,
                        ["stateName"] = settings.StateName,
                        ["previousStateId"] = settings.PreviousStateId,
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
                    Title = "TrackContactsJourney",
                    EventType = "ActionExecution",
                    StateId = context.GetCurrentStateId(),
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["stateId"] = settings.StateId,
                        ["stateName"] = settings.StateName,
                        ["previousStateId"] = settings.PreviousStateId,
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
    }
}
