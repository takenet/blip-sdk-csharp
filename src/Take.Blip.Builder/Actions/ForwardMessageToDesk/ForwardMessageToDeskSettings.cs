using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.ForwardMessageToDesk
{
    public class ForwardMessageToDeskSettings : IValidable
    {
        public string DeskDomain { get; set; }

        public void Validate()
        {
            
        }
    }
}
