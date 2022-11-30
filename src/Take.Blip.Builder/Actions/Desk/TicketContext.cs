using System.Runtime.Serialization;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder.Actions.Desk
{
    /// <summary>
    /// Represents a ticket saved on user context.
    /// </summary>
    [DataContract]
    public class TicketContext
    {
        public TicketContext()
        {

        }

        public TicketContext(Ticket ticket)
        {
            Id = ticket.Id;
            SequentialId = ticket.SequentialId;
            SequentialSuffix = ticket.SequentialSuffix;
            OwnerIdentity = ticket.OwnerIdentity.ToString();
            CustomerIdentity = ticket.CustomerIdentity.ToString();
        }

        /// <summary>
        /// The ticket unique id.
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// The ticket sequential id.
        /// </summary>
        [DataMember(Name = "sequentialId")]
        public int SequentialId { get; set; }

        /// <summary>
        /// The ticket sufix for sequential id.
        /// </summary>
        [DataMember(Name = "sequentialSuffix")]
        public string SequentialSuffix { get; set; }

        /// <summary>
        /// The identity of the ticket owner.
        /// </summary>
        [DataMember(Name = "ownerIdentity")]
        public string OwnerIdentity { get; set; }

        /// <summary>
        /// The identity of the customer.
        /// </summary>
        [DataMember(Name = "customerIdentity")]
        public string CustomerIdentity { get; set; }
    }
}
