using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Client.Extensions.HelpDesk
{
    /// <summary>
    /// Provide human attendance forwarding, using BLiP HelpDesks module
    /// </summary>
    public interface IHelpDeskExtension
    {
        /// <summary>
        /// Forward a message to active HelpDesk application.
        /// </summary>
        /// <param name="message">The message to be forwarded to BLIP HelpDesk agents</param>
        Task ForwardMessageToAgentAsync(Message message, CancellationToken cancellationToken);

        /// <summary>
        /// Check if a message is a reply from a BLIP HelpDesks application 
        /// </summary>
        /// <param name="message">The Message that must be analyzed</param>
        bool IsFromAgent(Message message);

        /// <summary>
        /// Check if a message is a reply from a BLIP HelpDesks application 
        /// </summary>
        /// <param name="customerIdentity">The customer identity</param>
        /// <param name="context">The document to be send to agent as an initial context</param>
        Task<Ticket> CreateTicketAsync(Identity customerIdentity, Document context, CancellationToken cancellationToken);

        /// <summary>
        /// Create a ticket.
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Ticket> CreateTicketAsync(Ticket ticket, CancellationToken cancellationToken);
        
        /// <summary>
        /// Close ticket as a user
        /// </summary>
        /// <param name="ticketId">The Ticket ID to be closed</param>
        Task CloseTicketAsUser(string ticketId, CancellationToken cancellationToken);

        /// <summary>
        /// Get user open ticket if any
        /// </summary>
        /// <param name="customerIdentity">The customer identity</param>
        Task<Ticket> GetUserOpenTicketsAsync(Identity customerIdentity, CancellationToken cancellationToken);
    }
}