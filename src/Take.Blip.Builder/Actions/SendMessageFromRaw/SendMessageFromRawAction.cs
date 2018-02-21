using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Serialization;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Actions.ProcessHttp;
using Take.Blip.Builder.Models;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.SendMessageFromRaw
{
    public class SendMessageFromRawAction : IAction
    {
        private readonly ISender _sender;
        private readonly IDocumentSerializer _documentSerializer;

        public SendMessageFromRawAction(ISender sender, IDocumentSerializer documentSerializer)
        {
            _sender = sender;
            _documentSerializer = documentSerializer;
        }

        public string Type => nameof(SendMessageFromRaw);

        public Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var sendMessageFromRawSettings = settings.ToObject<SendMessageFromRawSettings>();

            throw new NotImplementedException();
        }
    }

   
}