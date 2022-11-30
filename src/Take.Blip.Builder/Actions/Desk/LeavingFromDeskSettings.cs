using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.ForwardMessageToDesk
{
    public class LeavingFromDeskSettings : IValidable
    {
        public bool DoNotCloseTicket { get; set; }

        public void Validate()
        {
            
        }
    }
}
