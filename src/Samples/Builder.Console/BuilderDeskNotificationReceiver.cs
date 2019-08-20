using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Take.Blip.Builder;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.HelpDesk;
using Takenet.Iris.Messaging.Resources;

namespace Builder.Console
{
    public class BuilderDeskNotificationReceiver : INotificationReceiver
    {
        private readonly BuilderSettings _settings;
        private readonly IStateManager _stateManager;
        private readonly ISender _sender;
        private readonly IContextProvider _contextProvider;
        private readonly IHelpDeskExtension _helpDeskExtension;
        private readonly IUserOwnerResolver _userOwnerResolver;
        private readonly LazyInput _lazyInput;

        public BuilderDeskNotificationReceiver(
            BuilderSettings settings,
            IStateManager stateManager,
            ISender sender,
            IContextProvider contextProvider,
            IDocumentSerializer documentSerializer,
            IEnvelopeSerializer envelopeSerializer,
            IArtificialIntelligenceExtension artificialIntelligenceExtension,
            IHelpDeskExtension helpDeskExtension,
            IUserOwnerResolver userOwnerResolver)
        {
            _settings = settings;
            _stateManager = stateManager;
            _sender = sender;
            _contextProvider = contextProvider;
            _helpDeskExtension = helpDeskExtension;
            _userOwnerResolver = userOwnerResolver;
            _lazyInput = new LazyInput(
                new Message(),
                new Identity(),
                _settings.Flow.BuilderConfiguration,
                documentSerializer,
                envelopeSerializer,
                artificialIntelligenceExtension,
                CancellationToken.None);
        }

        public async Task ReceiveAsync(Notification notification, CancellationToken cancellationToken)
        {
            // Ignore intermediate notifications (Accepted, Dispatched)
            if (notification.Event == Event.Accepted ||
                notification.Event == Event.Dispatched ||
                notification.From?.Name == null ||
                notification.From?.Domain == null ||
                notification.From.Name == "postmaster")
            {
                return;
            }

            // Check if there's an open ticket for the notification user
            var ticket = await GetTicketAsync(notification, cancellationToken);
            if (ticket == null) return;

            // Clone the notification and change the routing information
            var deskNotification = notification.ShallowCopy();
            deskNotification.From = null;

            if (notification.From.Domain.Equals(HelpDeskExtension.DEFAULT_DESK_DOMAIN, StringComparison.OrdinalIgnoreCase))
            {
                deskNotification.To = (ticket.RoutingCustomerIdentity ?? ticket.CustomerIdentity).ToNode();
                deskNotification.Metadata = new Dictionary<string, string>
                {
                    {"#message.to", deskNotification.From},
                    {"desk.ticketId", ticket.Id}
                };
            }
            else
            {
                deskNotification.To = Identity.Parse($"{ticket.Id}@{HelpDeskExtension.DEFAULT_DESK_DOMAIN}").ToNode();
                deskNotification.Metadata = new Dictionary<string, string>
                {
                    {"#message.to", $"{ticket.Id}@{HelpDeskExtension.DEFAULT_DESK_DOMAIN}"}
                };
            }

            await _sender.SendNotificationAsync(deskNotification, cancellationToken);
        }

        private async Task<Ticket> GetTicketAsync(Notification notification, CancellationToken cancellationToken)
        {
            var (userIdentity, ownerIdentity) = await _userOwnerResolver.GetUserOwnerIdentitiesAsync(
                notification, _settings.Flow.BuilderConfiguration, cancellationToken);

            // If the notification is from 'desk.msging.net' domain, the user identity is encoded in the node 'name' property.
            if (notification.From.Domain.Equals(HelpDeskExtension.DEFAULT_DESK_DOMAIN,
                    StringComparison.OrdinalIgnoreCase) &&
                Identity.TryParse(Uri.UnescapeDataString(notification.From.Name), out var encodedUserIdentity))
            {
                userIdentity = encodedUserIdentity;
            }

            var context = _contextProvider.CreateContext(userIdentity, ownerIdentity, _lazyInput, _settings.Flow);

            // Check if the user is in a desk state
            var stateId = await _stateManager.GetStateIdAsync(context, cancellationToken);
            if (stateId == null || !stateId.StartsWith("desk:")) return null;

            var ticket = await GetCustomerActiveTicketAsync(userIdentity, cancellationToken);
            if (ticket == null &&
                _settings.Flow.BuilderConfiguration?.UseTunnelOwnerContext == true)
            {
                // This is just to support obsolete desk states with router context.
                // If you are seeing this and the current year is no longer 2019, you can safely remove this.
                var customerIdentity = notification.From.ToIdentity();
                ticket = await GetCustomerActiveTicketAsync(customerIdentity, cancellationToken);
            }

            return ticket;
        }

        private Task<Ticket> GetCustomerActiveTicketAsync(Identity userIdentity, CancellationToken cancellationToken)
        {
            return _helpDeskExtension.GetCustomerActiveTicketAsync(userIdentity, cancellationToken);
        }
    }
}