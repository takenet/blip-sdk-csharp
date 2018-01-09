using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Builder.Actions.SendMessageFromHttp
{
    public class SendMessageFromHttpSettings
    {
        public string Type { get; set; }

        public Uri Uri { get; set; }

        public Dictionary<string, string> Headers { get; set; }
    }
}
