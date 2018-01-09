using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Utils;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.SendMessageFromHttp
{
    /// <summary>
    /// Allow sending a message with the content retrieved from a HTTP URL.
    /// </summary>
    public class SendMessageFromHttpAction : IAction
    {
        private readonly ISender _sender;
        private readonly IHttpClient _httpClient;
        private readonly IDocumentSerializer _documentSerializer;

        public SendMessageFromHttpAction(ISender sender, IHttpClient httpClient, IDocumentSerializer documentSerializer)
        {
            _sender = sender;
            _httpClient = httpClient;
            _documentSerializer = documentSerializer;
        }

        public string Type => nameof(SendMessageFromHttp);

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var sendMessageFromHttpSettings = settings.ToObject<SendMessageFromHttpSettings>();
            if (sendMessageFromHttpSettings.Uri == null)
            {
                throw new ArgumentException(
                    $"The '{nameof(SendMessageFromHttpSettings.Uri)}' settings value is required for '{nameof(SendMessageFromHttpAction)}' action");
            }
            if (sendMessageFromHttpSettings.Type == null)
            {
                throw new ArgumentException(
                    $"The '{nameof(SendMessageFromHttpSettings.Type)}' settings value is required for '{nameof(SendMessageFromHttpAction)}' action");
            }
            if (!MediaType.TryParse(sendMessageFromHttpSettings.Type, out var mediaType))
            {
                throw new ArgumentException(
                    $"The '{nameof(SendMessageFromHttpSettings.Type)}' settings value must be a valid MIME type");
            }

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, sendMessageFromHttpSettings.Uri);            
            if (sendMessageFromHttpSettings.Headers != null)
            {
                foreach (var header in sendMessageFromHttpSettings.Headers)
                {
                    httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            else
            {
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(sendMessageFromHttpSettings.Type));
            }

            var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
            httpResponseMessage.EnsureSuccessStatusCode();

            var body = await httpResponseMessage.Content.ReadAsStringAsync();
            var message = new Message(EnvelopeId.NewId())
            {
                Id = EnvelopeId.NewId(),
                To = context.User.ToNode(),
                Content = _documentSerializer.Deserialize(body, mediaType)
            };

            await _sender.SendMessageAsync(message, cancellationToken);
        }
    }
}
