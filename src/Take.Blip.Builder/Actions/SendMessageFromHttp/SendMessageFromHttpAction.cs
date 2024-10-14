using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Take.Blip.Builder.Utils;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.SendMessageFromHttp
{
    /// <summary>
    /// Allow sending a message with the content retrieved from a HTTP URL.
    /// </summary>
    public class SendMessageFromHttpAction : ActionBase<SendMessageFromHttpSettings>
    {
        public static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(60);

        private readonly ISender _sender;
        private readonly IHttpClient _httpClient;
        private readonly IDocumentSerializer _documentSerializer;

        public SendMessageFromHttpAction(ISender sender, IHttpClient httpClient, IDocumentSerializer documentSerializer)
            : base(nameof(SendMessageFromHttp))
        {
            _sender = sender;
            _httpClient = httpClient;
            _documentSerializer = documentSerializer;
        }

        public override async Task ExecuteAsync(IContext context, SendMessageFromHttpSettings settings, CancellationToken cancellationToken)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, settings.Uri);
            if (settings.Headers != null)
            {
                foreach (var header in settings.Headers)
                {
                    httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            else
            {
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(settings.Type));
            }

            using (var cts = new CancellationTokenSource(settings.RequestTimeout ?? DefaultRequestTimeout))
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
            using (var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage, linkedCts.Token).ConfigureAwait(false))
            {
                httpResponseMessage.EnsureSuccessStatusCode();

                var body = await httpResponseMessage.Content.ReadAsStringAsync();
                var message = new Message(EnvelopeId.NewId())
                {
                    Id = EnvelopeId.NewId(),
                    To = context.Input.Message.From,
                    Content = _documentSerializer.Deserialize(body, settings.MediaType)
                };

                if (context.Input.Message.From.Domain.Equals("tunnel.msging.net"))
                {
                    message.Metadata ??= new Dictionary<string, string>();

                    if (context.Input.Message.Metadata.TryGetValue("#tunnel.owner", out string owner))
                    {
                        message.Metadata.Add("#tunnel.owner", owner);
                    }

                    if (context.Input.Message.Metadata.TryGetValue("#tunnel.originator", out string originator))
                    {
                        message.Metadata.Add("#tunnel.originator", originator);
                    }
                }

                await _sender.SendMessageAsync(message, cancellationToken);
            }
        }
    }
}
