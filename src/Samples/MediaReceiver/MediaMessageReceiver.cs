using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using System.Diagnostics;
using Take.Blip.Client;
using Lime.Messaging.Contents;

namespace MediaReceiver
{
    /// <summary>
    /// Defines a class for handling messages. 
    /// This type must be registered in the application.json file in the 'messageReceivers' section.
    /// </summary>
    public class MediaMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private readonly Settings _settings;

        public MediaMessageReceiver(ISender sender, Settings settings)
        {
            _sender = sender;
            _settings = settings;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            //Safe cast because this Receiver receives only messages with MediaLink content
            var mediaContent = message.Content as MediaLink;
            var response = $@"Received a media with following properties: 

                Uri: {mediaContent.Uri}
                Type: {mediaContent.Type}
                Title: {mediaContent.Title}
                Text: {mediaContent.Text}";


            Trace.TraceInformation($"From: {message.From} \tContent: {message.Content}");
            await _sender.SendMessageAsync(response, message.From, cancellationToken);
        }
    }
}
