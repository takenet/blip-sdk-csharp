using System.Collections.Generic;

namespace Take.Blip.Builder.Actions.SendMessageFromRaw
{
    public class SendMessageFromRawSettings
    {
        public string Type { get; set; }

        public string RawContent { get; set; }

        public Dictionary<string, string> Metadata { get; set; }
    }
}
