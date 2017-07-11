using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;

namespace Take.Blip.Client.Receivers
{

    public class UnsupportedCommandReceiver : UnsupportedEnvelopeReceiver<Command>
    {
        public UnsupportedCommandReceiver() : base(
            new Reason
            {
                Code = ReasonCodes.COMMAND_RESOURCE_NOT_SUPPORTED,
                Description = "There's no resource processor available to handle the received command"
            })
        { }
    }
}