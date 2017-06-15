using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Facts.Conditions
{

    public class EnvelopeReceivedConditionFilter
    {
        public string Sender { get; set; }

        public string Destination { get; set; }
    }
}
