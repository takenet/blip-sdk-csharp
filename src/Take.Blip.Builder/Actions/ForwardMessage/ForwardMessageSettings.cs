using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.ForwardMessage
{
    public class ForwardToAgentSettings : IValidable
    {
        public string Domain { get; set; }

        public ForwardToAgentSettingsContext Context { get; set; }

        public void Validate()
        {
            
        }
    }

    public class ForwardToAgentSettingsContext
    {
        public string Type { get; set; }

        public JToken Content { get; set; }
    }
}
