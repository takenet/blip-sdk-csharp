using System;
using System.Collections.Generic;

namespace Take.Blip.Builder.Actions.ProcessHttp
{
    public class ProcessHttpSettings
    {
        public string Method { get; set; }

        public Uri Uri { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string Body { get; set; }

        public string BodyVariable { get; set; }

        public string StatusVariable { get; set; }
    }
}