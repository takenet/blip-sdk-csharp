using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Take.Blip.Client.Activation;

namespace Take.Blip.Client.Web
{
    /// <summary>
    /// Emulates a transport connection thought the HTTP interface
    /// </summary>
    public sealed class WebTransport : TransportBase, ITransport
    {
        private readonly IEnvelopeBuffer _envelopeBuffer;
        private readonly IEnvelopeSerializer _serializer;
        private readonly Application _application;
        private readonly string _baseUri;
        private readonly HttpClient _client;

        public WebTransport(IEnvelopeBuffer envelopeBuffer, IEnvelopeSerializer serializer, Application application, Uri baseUri)
        {
            _envelopeBuffer = envelopeBuffer;
            _serializer = serializer;
            _application = application;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Key", GetAuthCredentials(application));
            _baseUri = baseUri.ToString().TrimEnd('/');
        }

        public override async Task SendAsync(Envelope envelope, CancellationToken cancellationToken)
        {
            try
            {
                switch (envelope)
                {
                    case Message message:
                        await SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
                        return;
                    case Notification notification:
                        await SendNotificationAsync(notification, cancellationToken).ConfigureAwait(false);
                        return;
                    case Command command:
                        await SendCommandAsync(command, cancellationToken).ConfigureAwait(false);
                        return;
                    case Lime.Protocol.Session session:
                        await HandleSessionAsync(session, cancellationToken).ConfigureAwait(false); 
                        return;
                }
            }
            catch (Exception ex)
            {
                var session = CreateSession();
                session.State = SessionState.Failed;
                session.Reason = ex.ToReason();
                await _envelopeBuffer.SendAsync(session, cancellationToken).ConfigureAwait(false);
                throw;
            }

            throw new NotSupportedException("Unknown envelope type");
        }

        public override Task<Envelope> ReceiveAsync(CancellationToken cancellationToken) 
            => _envelopeBuffer.ReceiveAsync(cancellationToken);

        protected override Task PerformCloseAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        protected override Task PerformOpenAsync(Uri uri, CancellationToken cancellationToken) => Task.CompletedTask;

        public override bool IsConnected => true;

        public void Dispose()
        {
            _client.Dispose();
        }

        private async Task SendMessageAsync(Message message, CancellationToken cancellationToken)
        {
            using (var content = GetContent(message))
            using (var response = await _client.PostAsync($"{_baseUri}/messages", content, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        private async Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken)
        {
            using (var content = GetContent(notification))
            using (var response = await _client.PostAsync($"{_baseUri}/notifications", content, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        private async Task SendCommandAsync(Command command, CancellationToken cancellationToken)
        {
            // Give fake responses for these specific commands that are not supported by the HTTP interface
            if (command.IsPingRequest()
                || (command.Method == CommandMethod.Set
                    && (command.Uri.ToString() == "/presence" 
                        || command.Uri.ToString() == "/receipt")))
            {
                await _envelopeBuffer.SendAsync(command.CreateSuccessResponse(), cancellationToken);
                return;
            }
            
            using (var content = GetContent(command))
            using (var response = await _client.PostAsync($"{_baseUri}/commands", content, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                if (response.Content != null)
                {
                    await _envelopeBuffer.SendAsync(
                        _serializer.Deserialize((await response.Content.ReadAsStringAsync().ConfigureAwait(false))),
                        cancellationToken);
                }
            }
        }

        private HttpContent GetContent(Envelope envelope) =>
            new StringContent(_serializer.Serialize(envelope), Encoding.UTF8, "application/json");


        private async Task HandleSessionAsync(Lime.Protocol.Session session, CancellationToken cancellationToken)
        {
            // We dont actually send the session envelopes throught the HTTP interface, 
            // but emulate the behavior accordingly to the channel state
            var stateSession = CreateSession();

            if (session.State == SessionState.New)
            {
                stateSession.State = SessionState.Established;
            }
            else if (session.State == SessionState.Finishing)
            {
                stateSession.State = SessionState.Finished;
            }

            await _envelopeBuffer.SendAsync(stateSession, cancellationToken).ConfigureAwait(false);
        }

        private Lime.Protocol.Session CreateSession()
        {
            return new Lime.Protocol.Session
            {
                Id = Guid.NewGuid().ToString(),
                From = $"postmaster@{_application.Domain}/fake",
                To = $"{_application.Identifier}@{_application.Domain}/fake"
            };
        }

        private static string GetAuthCredentials(Application applicationSettings) =>
            $"{applicationSettings.Identifier}:{applicationSettings.AccessKey.FromBase64()}".ToBase64();
    }
}
