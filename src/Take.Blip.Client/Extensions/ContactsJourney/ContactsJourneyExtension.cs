using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Iris.Messaging.Resources.Analytics;

namespace Take.Blip.Client.Extensions.ContactsJourney
{
    public class ContactsJourneyExtension : ExtensionBase, IContactsJourneyExtension
    {
        private const string CONTACTS_JOURNEY_URI = "/contacts-journey";

        private readonly Node AnalyticsAddress = Node.Parse("postmaster@analytics.msging.net");

        public ContactsJourneyExtension(ISender sender)
            : base(sender)
        {
        }

        public Task AddAsync(
            string stateId, 
            string stateName, 
            string previousStateId = null, 
            string previousStateName = null, 
            string contactIdentity = null, 
            bool fireAndForget = false, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(stateId)) throw new ArgumentNullException(nameof(stateId));
            if (string.IsNullOrEmpty(stateName)) throw new ArgumentNullException(nameof(stateName));

            var requestCommand = new Command(null)
            {
                Uri = new LimeUri(CONTACTS_JOURNEY_URI),
                To = AnalyticsAddress,
                Resource = new ContactsJourneyNode
                {
                    CurrentStateId = stateId,
                    CurrentStateName = stateName,
                    PreviousStateId = previousStateId,
                    PreviousStateName = previousStateName,
                    ContactIdentity = contactIdentity
                }
            };

            if (fireAndForget)
            {
                requestCommand.Method = CommandMethod.Observe;
                return Sender.SendCommandAsync(requestCommand, cancellationToken);
            }

            requestCommand.Id = EnvelopeId.NewId();
            requestCommand.Method = CommandMethod.Set;

            return ProcessCommandAsync(requestCommand, cancellationToken);
        }
    }
}
