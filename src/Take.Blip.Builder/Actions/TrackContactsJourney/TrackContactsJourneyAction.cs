using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Client.Extensions.ContactsJourney;

namespace Take.Blip.Builder.Actions.TrackContactsJourney
{
    public class TrackContactsJourneyAction : ActionBase<TrackContactsJourneySettings>
    {
        private readonly IContactsJourneyExtension _contactsJourneyExtension;

        public TrackContactsJourneyAction(IContactsJourneyExtension contactsJourneyExtension)
            : base(nameof(TrackContactsJourney))
        {
            _contactsJourneyExtension = contactsJourneyExtension;
        }

        public override Task ExecuteAsync(IContext context, TrackContactsJourneySettings settings, CancellationToken cancellationToken)
        {
            return _contactsJourneyExtension.AddAsync(
                settings.StateId,
                settings.StateName,
                settings.PreviousStateId,
                settings.PreviousStateName,
                contactIdentity: context.UserIdentity,
                fireAndForget: settings.FireAndForget ?? true,
                cancellationToken: cancellationToken
                );
        }
    }
}
