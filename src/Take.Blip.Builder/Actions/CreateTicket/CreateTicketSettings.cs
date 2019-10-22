using Lime.Protocol;
using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder.Actions.CreateTicket
{
    public class CreateTicketSettings : IValidable
    {
        public string OwnerIdentity { get; set; }

        public string RoutingOwnerIdentity { get; set; }

        public string CustomerIdentity { get; set; }

        public string RoutingCustomerIdentity { get; set; }

        public string Variable { get; set; }

        public DocumentContainer CustomerInput { get; set; }

        public void Validate()
        {
        }
    }
}