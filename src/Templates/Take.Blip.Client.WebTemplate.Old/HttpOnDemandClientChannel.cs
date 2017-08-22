using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Takenet.MessagingHub.Client.Host;

namespace Take.Blip.Client.WebTemplate.Old
{
    internal class HttpOnDemandClientChannel : IOnDemandClientChannel, IDisposable
    {
        private const string WebhookKeyConfigurationName = "MessagingHub.WebhookKey";
        private const string BaseUrlConfigurationName = "MessagingHub.BaseUrl";
        private const string DefaultBaseUrl = "https://msging.net";

        private readonly IEnvelopeSerializer _serializer;
        private readonly IEnvelopeBuffer _envelopeBuffer;
        private readonly string _baseUrl;
        private readonly HttpClient _client;
        private readonly Application _applicationSettings;
        private readonly string _webhookKey;

        public HttpOnDemandClientChannel(IEnvelopeBuffer envelopeBuffer, IEnvelopeSerializer serializer, Application applicationSettings)
        {
            _envelopeBuffer = envelopeBuffer;
            _applicationSettings = applicationSettings;
            _serializer = serializer;
            _webhookKey = ConfigurationManager.AppSettings[WebhookKeyConfigurationName];
            _baseUrl = ConfigurationManager.AppSettings[BaseUrlConfigurationName];

            if (string.IsNullOrWhiteSpace(_baseUrl)) _baseUrl = DefaultBaseUrl;
            _baseUrl = _baseUrl.TrimEnd('/');

            CheckCredentialsOrThrow();

            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Key", _webhookKey ?? GetAuthCredentials(applicationSettings));
        }

        public ICollection<Func<ChannelInformation, Task>> ChannelCreatedHandlers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<Func<FailedChannelInformation, Task<bool>>> ChannelCreationFailedHandlers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<Func<ChannelInformation, Task>> ChannelDiscardedHandlers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<Func<FailedChannelInformation, Task<bool>>> ChannelOperationFailedHandlers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsEstablished { get; private set; }

        public Task EstablishAsync(CancellationToken cancellationToken)
        {
            IsEstablished = true;
            return Task.CompletedTask;
        }

        public Task FinishAsync(CancellationToken cancellationToken)
        {
            IsEstablished = false;
            return Task.CompletedTask;
        }

        public Task<Command> ReceiveCommandAsync(CancellationToken cancellationToken)
        {
            return _envelopeBuffer.Commands.ReceiveAsync(cancellationToken);
        }

        public Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            return _envelopeBuffer.Messages.ReceiveAsync(cancellationToken);
        }

        public Task<Notification> ReceiveNotificationAsync(CancellationToken cancellationToken)
        {
            return _envelopeBuffer.Notifications.ReceiveAsync(cancellationToken);
        }

        public async Task<Command> ProcessCommandAsync(Command command, CancellationToken cancellationToken)
        {
            using (var content = GetContent(command))
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/commands"))
            {
                request.Content = content;
                using (var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return (Command)_serializer.Deserialize(jsonResponse);
                }
            }
        }

        public async Task SendCommandAsync(Command command, CancellationToken cancellationToken)
        {
            using (var content = GetContent(command))
            using (var response = await _client.PostAsync($"{_baseUrl}/commands", content, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                _envelopeBuffer.Commands.Post(
                    _serializer.Deserialize((await response.Content.ReadAsStringAsync().ConfigureAwait(false))) as Command);
            }
        }

        public async Task SendMessageAsync(Message message, CancellationToken cancellationToken)
        {
            using (var content = GetContent(message))
            using (var response = await _client.PostAsync($"{_baseUrl}/messages", content, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken)
        {
            using(var content = GetContent(notification))
            using (var response = await _client.PostAsync($"{_baseUrl}/notifications", content, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private HttpContent GetContent(Envelope envelope) => 
            new StringContent(_serializer.Serialize(envelope), Encoding.UTF8, "application/json");

        private void CheckCredentialsOrThrow()
        {
            if (string.IsNullOrWhiteSpace(_webhookKey) &&
                (string.IsNullOrWhiteSpace(_applicationSettings.Identifier) || string.IsNullOrWhiteSpace(_applicationSettings.AccessKey)))
                throw new InvalidOperationException($"Define {WebhookKeyConfigurationName} key or the pair MessagingHub.{nameof(_applicationSettings.Identifier)}/MessagingHub.{nameof(_applicationSettings.AccessKey)} in your Web.config");
        }

        private string GetAuthCredentials(Application applicationSettings) => 
            $"{applicationSettings.Identifier}:{applicationSettings.AccessKey.FromBase64()}".ToBase64();
    }
}
