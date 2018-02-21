using System.Collections.Generic;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.SendMessageFromRaw
{
    public class SendMessageFromRawSettings : IValidable
    {
        public string Type { get; set; }

        public string RawContent { get; set; }

        public Dictionary<string, string> Metadata { get; set; }

        public void Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}
