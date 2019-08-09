using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder.Actions.CreateTicket
{
    public class CreateTicketSettings : Ticket, IValidable
    {
        public string Variable { get; set; }
        
        public void Validate()
        {

        }
    }
}