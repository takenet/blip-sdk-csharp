using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Serialization;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.SendMessageFromRaw
{
    public class SendMessageFromRawAction : ActionBase<SendMessageFromRawSettings>
    {
        private readonly ISender _sender;
        private readonly IDocumentSerializer _documentSerializer;

        public SendMessageFromRawAction(ISender sender, IDocumentSerializer documentSerializer)
            : base(nameof(SendMessageFromRaw))
        {
            _sender = sender;
            _documentSerializer = documentSerializer;
        }

        public override Task ExecuteAsync(IContext context, SendMessageFromRawSettings settings, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

   
}