using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace Take.Blip.Builder.PaymentBlock.Tests.Support
{
    /// <summary>
    /// <see cref="ISender"/> simulado: grava na sequência do <see cref="BuilderEngineHarness.InteractionLog"/> cada comando,
    /// mensagem e notificação que o motor envia e responde em memória aos comandos de leitura de bucket
    /// (<c>get /buckets/&lt;id&gt;</c>). Nada sai para a rede.
    /// </summary>
    public sealed class SimulatedSender : ISender
    {
        private const string BUCKETS_PREFIX = "/buckets/";

        private readonly BuilderEngineHarness.InteractionLog _log;
        private readonly ConcurrentDictionary<string, string> _buckets =
            new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Cria o sender simulado gravando na sequência informada.</summary>
        public SimulatedSender(BuilderEngineHarness.InteractionLog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>Guarda um texto no bucket <paramref name="bucketId"/>, devolvido em <c>get /buckets/&lt;id&gt;</c>.</summary>
        public void SetBucketText(string bucketId, string value)
        {
            if (string.IsNullOrWhiteSpace(bucketId))
            {
                throw new ArgumentException("O id do bucket é obrigatório.", nameof(bucketId));
            }

            _buckets[bucketId] = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public Task SendMessageAsync(Message message, CancellationToken cancellationToken)
        {
            Record(BuilderEngineHarness.InteractionKind.Message, message, cancellationToken);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken)
        {
            Record(BuilderEngineHarness.InteractionKind.Notification, notification, cancellationToken);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SendCommandAsync(Command command, CancellationToken cancellationToken)
        {
            Record(BuilderEngineHarness.InteractionKind.Command, command, cancellationToken);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<Command> ProcessCommandAsync(Command requestCommand, CancellationToken cancellationToken)
        {
            Record(BuilderEngineHarness.InteractionKind.Command, requestCommand, cancellationToken);
            return Task.FromResult(CreateResponse(requestCommand));
        }

        private void Record(BuilderEngineHarness.InteractionKind kind, Envelope envelope, CancellationToken cancellationToken)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

            cancellationToken.ThrowIfCancellationRequested();
            _log.Add(kind).Envelope = envelope;
        }

        private Command CreateResponse(Command request)
        {
            var response = new Command
            {
                Id = request.Id,
                From = request.To,
                To = request.From,
                Method = request.Method,
                Status = CommandStatus.Success,
            };

            var uri = request.Uri?.ToString();
            if (request.Method != CommandMethod.Get
                || uri == null
                || !uri.StartsWith(BUCKETS_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                return response;
            }

            if (_buckets.TryGetValue(uri.Substring(BUCKETS_PREFIX.Length), out var text))
            {
                response.Resource = new PlainText { Text = text };
                return response;
            }

            response.Status = CommandStatus.Failure;
            response.Reason = new Reason
            {
                Code = ReasonCodes.COMMAND_RESOURCE_NOT_FOUND,
                Description = "The requested resource was not found",
            };
            return response;
        }
    }
}
