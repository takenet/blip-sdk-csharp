using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
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
        private readonly IBlipLogger _blipMonitoringLogger;

        public SendMessageFromHttpAction(ISender sender, IHttpClient httpClient, IDocumentSerializer documentSerializer, IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(SendMessageFromHttp))
        {
            _sender = sender;
            _httpClient = httpClient;
            _documentSerializer = documentSerializer;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(IContext context, SendMessageFromHttpSettings settings, CancellationToken cancellationToken)
        {
            int responseStatus = 0;
            try
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
                    responseStatus = (int)httpResponseMessage.StatusCode;
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

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "SendMessageFromHttp",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["uri"] = settings.Uri?.ToString(),
                        ["responseStatus"] = responseStatus,
                        ["success"] = true,
                    },
                    FlowVersion = context.Flow?.Version,
                    Channel = context.Input.Message?.From?.Domain,
                    IdMessage = context.Input.Message?.Id,
                    From = context.UserIdentity?.ToString(),
                    To = context.OwnerIdentity?.ToString(),
                    OriginalFrom = context.Input.Message?.From,
                    OriginalTo = context.Input.Message?.To,
                });
            }
            catch (Exception ex)
            {
                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "SendMessageFromHttp",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["uri"] = settings.Uri?.ToString(),
                        ["responseStatus"] = responseStatus,
                        ["success"] = false,
                        ["error"] = ex.ToString(),
                    },
                    FlowVersion = context.Flow?.Version,
                    Channel = context.Input.Message?.From?.Domain,
                    IdMessage = context.Input.Message?.Id,
                    From = context.UserIdentity?.ToString(),
                    To = context.OwnerIdentity?.ToString(),
                    OriginalFrom = context.Input.Message?.From,
                    OriginalTo = context.Input.Message?.To,
                });
                throw;
            }
        }
    }
}
