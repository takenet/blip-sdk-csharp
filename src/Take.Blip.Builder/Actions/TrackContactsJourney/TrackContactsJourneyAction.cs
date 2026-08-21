using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Blip.Ai.Bot.Monitoring.Logging.Interface;
using Blip.Ai.Bot.Monitoring.Logging.Services;
using Newtonsoft.Json.Linq;
using Take.Blip.Client.Extensions.ContactsJourney;

namespace Take.Blip.Builder.Actions.TrackContactsJourney
{
    public class TrackContactsJourneyAction : ActionBase<TrackContactsJourneySettings>
    {
        private readonly IContactsJourneyExtension _contactsJourneyExtension;
        private readonly IBlipLogger _blipMonitoringLogger;

        public TrackContactsJourneyAction(
            IContactsJourneyExtension contactsJourneyExtension,
            IBlipLogger? blipMonitoringLogger = null
        )
            : base(nameof(TrackContactsJourney))
        {
            _contactsJourneyExtension = contactsJourneyExtension;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(
            IContext context,
            TrackContactsJourneySettings settings,
            CancellationToken cancellationToken
        )
        {
            var sw = Stopwatch.StartNew();
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

                this.LogExecution(
                    _blipMonitoringLogger,
                    context,
                    new JObject
                    {
                        ["stateId"] = settings.StateId,
                        ["stateName"] = settings.StateName,
                        ["previousStateId"] = settings.PreviousStateId,
                        ["previousStateName"] = settings.PreviousStateName,
                        ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                    }
                );
            }
            catch (Exception ex)
            {
                this.LogError(
                    _blipMonitoringLogger,
                    context,
                    new JObject
                    {
                        ["stateId"] = settings.StateId,
                        ["stateName"] = settings.StateName,
                        ["previousStateId"] = settings.PreviousStateId,
                        ["previousStateName"] = settings.PreviousStateName,
                        ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                    },
                    ex
                );
                throw;
            }
        }
    }
}
