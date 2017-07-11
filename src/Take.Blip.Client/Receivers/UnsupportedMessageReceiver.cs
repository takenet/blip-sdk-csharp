using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;

namespace Take.Blip.Client.Receivers
{
    /// <summary>
    /// Message receiver that automatically respond to any message as an unsupported message
    /// </summary>
    public class UnsupportedMessageReceiver : UnsupportedEnvelopeReceiver<Message>
    {
        public UnsupportedMessageReceiver() : base(
            new Reason
            {
                Code = ReasonCodes.MESSAGE_UNSUPPORTED_CONTENT_TYPE,
                Description = "There's no processor available to handle the received message"
            })
        { }
    }
}