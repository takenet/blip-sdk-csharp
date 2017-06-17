using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Activation.Rules.Conditions
{

    public class MessageReceivedConditionFilter : EnvelopeReceivedConditionFilter
    {
        public string Type { get; set; }

        public string Content { get; set; }
    }
}
