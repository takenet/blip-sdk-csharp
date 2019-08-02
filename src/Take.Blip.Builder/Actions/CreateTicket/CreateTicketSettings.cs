using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder.Actions.CreateTicket
{
    public class CreateTicketSettings : Ticket, IValidable
    {
        public void Validate()
        {

        }
    }
}