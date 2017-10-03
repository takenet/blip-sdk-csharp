using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionNativeContentReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private  readonly Dictionary<string,object> dictionary;

        public OptionNativeContentReceiver(ISender sender)
        {
            _sender = sender;
        }
        
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            dictionary.Add("text", "hello world!");


            var mediatype = new MediaType("application","json");
            var document = new JsonDocument( dictionary ,mediatype);
       

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }
    }

     public class NativeAirlineBoardingpassReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private  readonly Dictionary<string,object> dictionary;

        public NativeAirlineBoardingpassReceiver(ISender sender)
        {
            _sender = sender;
        }
        
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var mediatype = new MediaType("application","json");
        
      
            var document = new JsonDocument
            {

            };
       

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }
    }

}
