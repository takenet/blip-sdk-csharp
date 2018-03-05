using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.ForwardMessageToDesk
{
    public class ForwardMessageToDeskSettings : IValidable
    {
        public int? Expiration { get; set; }

        public string Domain { get; set; }

        public ForwardMessageToDeskSettingsContext Context { get; set; }

        public void Validate()
        {
            
        }
    }

    public class ForwardMessageToDeskSettingsContext
    {    
        public string Type { get; set; }

        public JToken Content { get; set; }
    }
}
