using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Extensions.ContactsJourney
{
    public interface IContactsJourneyExtension
    {
        Task AddAsync(
            string currentStateId,
            string currentStateName,
            string previousStateId = null,
            string previousStateName = null,
            string contactIdentity = null,
            bool fireAndForget = false,
            CancellationToken cancellationToken = new CancellationToken());
    }
}
