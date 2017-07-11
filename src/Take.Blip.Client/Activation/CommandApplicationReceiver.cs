using System;
using System.Collections.Generic;
using System.Text;
using Lime.Protocol;

namespace Take.Blip.Client.Activation
{
    public class CommandApplicationReceiver : ApplicationReceiver
    {
        public CommandMethod? Method { get; set; }

        public string Uri { get; set; }

        public string ResourceUri { get; set; }

        public string MediaType { get; set; }
    }
}
