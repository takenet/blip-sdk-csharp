using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blip.Ai.Bot.Monitoring.Logging.Interface;
using Lime.Messaging.Contents;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json.Linq;
using NSubstitute;
using SimpleInjector;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils;
using Take.Blip.Client;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.Broadcast;
using Take.Blip.Client.Extensions.Bucket;
using Take.Blip.Client.Extensions.Builder;
using Take.Blip.Client.Extensions.Contacts;
using Take.Blip.Client.Extensions.ContactsJourney;
using Take.Blip.Client.Extensions.Context;
using Take.Blip.Client.Extensions.EventTracker;
using Take.Blip.Client.Extensions.HelpDesk;
using Take.Blip.Client.Extensions.Scheduler;
using Take.Blip.Client.Extensions.Tunnel;

namespace Take.Blip.Builder.PaymentBlock.Tests.Support
{
    /// <summary>
    /// Executa um fluxo de referência no motor do Builder, no padrão de <c>FlowManagerTestsBase.CreateContainer</c>: um
    /// container por cenário com <c>RegisterBuilder()</c>, <see cref="ISender"/> e <see cref="IHttpClient"/> simulados,
    /// extensões substituídas e contexto do usuário em memória (sem Redis nem rede). Criado pela injeção de contexto do
    /// Reqnroll, uma instância por cenário, compartilhada pelos passos das features.
    /// </summary>
    public sealed class BuilderEngineHarness : IDisposable
    {
        private const string USER_STATE_VARIABLE_PREFIX = "stateId@";
        private static readonly TimeSpan INPUT_DEADLINE = TimeSpan.FromMinutes(2);

        private readonly InteractionLog _log = new InteractionLog();
        private readonly InMemoryContextExtension _contextExtension = new InMemoryContextExtension();
        private Container _container;
        private Flow _flow;
        private string _bucketSecret;
        private int _deliveredSchedules;

        /// <summary>Cria o harness com a checkout-api simulada no modo normal e o tempo de script padrão do motor.</summary>
        public BuilderEngineHarness()
        {
            Sender = new SimulatedSender(_log);
            HttpClient = new SimulatedHttpClient(_log);
            SchedulerExtension = Substitute.For<ISchedulerExtension>();
            EventTrackExtension = Substitute.For<IEventTrackExtension>();
            ContactExtension = Substitute.For<IContactExtension>();
            MonitoringLogger = Substitute.For<IBlipLogger>();
            ContactExtension
                .GetAsync(Arg.Any<Identity>(), Arg.Any<CancellationToken>())
                .Returns(call => new Contact { Identity = call.Arg<Identity>(), Name = SyntheticData.CONTACT_NAME });
            ScriptTimeout = TestBuilderConfiguration.DefaultExecuteScriptV2Timeout;
        }

        /// <summary>Tipo de cada item gravado na sequência.</summary>
        public enum InteractionKind
        {
            /// <summary>Comando enviado ou processado pelo motor (<see cref="Interaction.Envelope"/> é um <see cref="Command"/>).</summary>
            Command,

            /// <summary>Mensagem enviada pelo motor (<see cref="Interaction.Envelope"/> é uma <see cref="Message"/>).</summary>
            Message,

            /// <summary>Notificação enviada pelo motor.</summary>
            Notification,

            /// <summary>Chamada HTTP feita por <c>request.fetchAsync</c>.</summary>
            Http,
        }

        /// <summary><see cref="ISender"/> simulado registrado no container.</summary>
        public SimulatedSender Sender { get; }

        /// <summary><see cref="IHttpClient"/> simulado registrado no container.</summary>
        public SimulatedHttpClient HttpClient { get; }

        /// <summary>Extensão de agendamento substituída (expiração de input).</summary>
        public ISchedulerExtension SchedulerExtension { get; }

        /// <summary>Extensão de eventos substituída (ação TrackEvent).</summary>
        public IEventTrackExtension EventTrackExtension { get; }

        /// <summary>Extensão de contatos substituída, que devolve o contato sintético.</summary>
        public IContactExtension ContactExtension { get; }

        /// <summary>Logger de monitoramento do motor, substituído.</summary>
        public IBlipLogger MonitoringLogger { get; }

        /// <summary>Tempo máximo de <c>ExecuteScriptV2</c>; vale para o container criado no próximo envio.</summary>
        public TimeSpan ScriptTimeout { get; set; }

        /// <summary>Fluxo de referência em uso (nome da fixture).</summary>
        public string ReferenceFlow { get; private set; }

        /// <summary>Configuração do bloco em uso (coluna <c>configuracao</c> de <c>configuracoes-v1.json</c>).</summary>
        public string ConfigurationName { get; private set; }

        /// <summary>Exceção devolvida pelo motor no último envio, ou <c>null</c> se ele terminou sem erro.</summary>
        public Exception EngineFailure { get; private set; }

        /// <summary>Sequência ordenada de comandos, mensagens, notificações e chamadas HTTP gravados até agora.</summary>
        public IReadOnlyList<Interaction> Interactions => _log.Snapshot();

        /// <summary>Segredo do bucket e token da auth-api gerados nesta execução, para o normalizador mascarar.</summary>
        public IReadOnlyList<string> RunSecrets
            => _bucketSecret == null ? new[] { HttpClient.BotToken } : new[] { _bucketSecret, HttpClient.BotToken };

        /// <summary>Escolhe o fluxo de referência e a linha de configuração usados no próximo envio.</summary>
        public void UseReferenceFlow(string referenceFlow, string configuration, bool feedback)
        {
            var row = FlowAssembler.LoadConfiguration(configuration, feedback);
            _flow = FlowAssembler.Assemble(referenceFlow, row);
            HttpClient.HasSavedCard = (bool?)row["cartaoSalvo"] ?? false;
            ReferenceFlow = referenceFlow;
            ConfigurationName = configuration;
        }

        /// <summary>Gera o segredo desta execução, guarda no bucket <paramref name="bucketId"/> e o faz aceito pela auth-api.</summary>
        public void StoreBucketSecret(string bucketId)
        {
            _bucketSecret = SyntheticData.NewRunSecret();
            Sender.SetBucketText(bucketId, _bucketSecret);
            HttpClient.AcceptedSecret = _bucketSecret;
        }

        /// <summary>Indica se <paramref name="value"/> é o segredo do bucket desta execução, sem expor o valor.</summary>
        public bool IsBucketSecret(string value)
            => _bucketSecret != null && string.Equals(value, _bucketSecret, StringComparison.Ordinal);

        /// <summary>Envia ao motor a mensagem do cliente que leva o fluxo ao bloco e guarda a falha, se houver.</summary>
        public Task SendClientMessageAsync()
        {
            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                From = Node.Parse(SyntheticData.USER_IDENTITY),
                To = ApplicationNode(),
                Content = new PlainText { Text = SyntheticData.CLIENT_MESSAGE_TEXT },
                Metadata = new Dictionary<string, string>(),
            };

            return ProcessAsync(message);
        }

        /// <summary>
        /// Entrega ao motor, sem espera real, a expiração de input que ele agendou no <see cref="SchedulerExtension"/> depois
        /// da última entrega, como o agendador faria ao fim do tempo, e guarda a falha, se houver.
        /// </summary>
        public Task DeliverScheduledInputExpirationAsync()
        {
            Message scheduled = null;
            Node scheduledFrom = null;
            var schedules = 0;
            foreach (var call in SchedulerExtension.ReceivedCalls())
            {
                var arguments = call.GetArguments();
                if (string.Equals(call.GetMethodInfo().Name, nameof(ISchedulerExtension.ScheduleMessageAsync), StringComparison.Ordinal)
                    && arguments.Length > 0
                    && arguments[0] is Message message)
                {
                    schedules++;
                    scheduled = message;
                    scheduledFrom = arguments.Length > 2 ? arguments[2] as Node : null;
                }
            }

            if (scheduled == null || schedules <= _deliveredSchedules)
            {
                throw new InvalidOperationException("O motor não agendou uma nova expiração de input desde a última entrega.");
            }

            _deliveredSchedules = schedules;
            return ProcessAsync(new Message(scheduled.Id)
            {
                From = scheduledFrom ?? ApplicationNode(),
                To = scheduled.To,
                Content = scheduled.Content,
                Metadata = scheduled.Metadata,
            });
        }

        private async Task ProcessAsync(Message message)
        {
            if (_flow == null)
            {
                throw new InvalidOperationException("Escolha o fluxo de referência antes de enviar a mensagem.");
            }

            _container ??= CreateContainer();
            EngineFailure = null;
            using (var deadline = new CancellationTokenSource(INPUT_DEADLINE))
            {
                try
                {
                    await _container.GetInstance<IFlowManager>().ProcessInputAsync(message, _flow, deadline.Token);
                }
                catch (Exception ex)
                {
                    EngineFailure = ex;
                }
            }
        }

        /// <summary>Estado do cliente no fluxo gravado pelo motor, ou <c>null</c> se não há estado gravado.</summary>
        public string UserStateId => GetContextVariable(USER_STATE_VARIABLE_PREFIX + SyntheticData.FLOW_ID);

        /// <summary>Valor atual da variável de contexto do cliente, ou <c>null</c> se ela não existe.</summary>
        public string GetContextVariable(string name)
            => _contextExtension.GetText(Identity.Parse(SyntheticData.USER_IDENTITY), name);

        /// <summary>Serializa o envelope com o serializador do motor, como ele iria para o servidor.</summary>
        public JObject ToJson(Envelope envelope)
        {
            if (_container == null)
            {
                throw new InvalidOperationException("O motor ainda não foi executado neste cenário.");
            }

            return JObject.Parse(_container.GetInstance<IEnvelopeSerializer>().Serialize(envelope));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _container?.Dispose();
        }

        private static Node ApplicationNode()
            => new Node(SyntheticData.APPLICATION_IDENTIFIER, SyntheticData.APPLICATION_DOMAIN, null);

        private Container CreateContainer()
        {
            var container = new Container();
            container.Options.EnableAutoVerification = false;
            container.Options.SuppressLifestyleMismatchVerification = true;
            container.Options.AllowOverridingRegistrations = true;
            container.RegisterBuilder();

            var configuration = new TestBuilderConfiguration(ScriptTimeout);
            var application = new Application
            {
                Identifier = SyntheticData.APPLICATION_IDENTIFIER,
                Domain = SyntheticData.APPLICATION_DOMAIN,
            };

            container.RegisterInstance<IServiceProvider>(container);
            container.RegisterSingleton<IConfiguration>(() => configuration);
            container.RegisterSingleton(() => application);
            container.RegisterSingleton<ISender>(() => Sender);
            container.RegisterSingleton<IHttpClient>(() => HttpClient);
            container.RegisterSingleton<IContextExtension>(() => _contextExtension);
            container.RegisterSingleton(() => SchedulerExtension);
            container.RegisterSingleton(() => EventTrackExtension);
            container.RegisterSingleton(() => ContactExtension);
            container.RegisterSingleton(() => MonitoringLogger);
            container.RegisterSingleton(() => Substitute.For<IBucketExtension>());
            container.RegisterSingleton(() => Substitute.For<IArtificialIntelligenceExtension>());
            container.RegisterSingleton(() => Substitute.For<IContactsJourneyExtension>());
            container.RegisterSingleton(() => Substitute.For<IBroadcastExtension>());
            container.RegisterSingleton(() => Substitute.For<IHelpDeskExtension>());
            container.RegisterSingleton(() => Substitute.For<ITunnelExtension>());
            container.RegisterSingleton(() => Substitute.For<IBuilderExtension>());
            return container;
        }

        /// <summary>Item da sequência gravada pelos simuladores.</summary>
        public sealed class Interaction
        {
            internal Interaction(int sequence, InteractionKind kind)
            {
                Sequence = sequence;
                Kind = kind;
            }

            /// <summary>Posição na sequência, a partir de 1.</summary>
            public int Sequence { get; }

            /// <summary>Tipo do item.</summary>
            public InteractionKind Kind { get; }

            /// <summary>Envelope enviado pelo motor (comando, mensagem ou notificação).</summary>
            public Envelope Envelope { get; internal set; }

            /// <summary>Método HTTP.</summary>
            public string HttpMethod { get; internal set; }

            /// <summary>URL chamada.</summary>
            public Uri HttpUri { get; internal set; }

            /// <summary>Cabeçalhos da requisição.</summary>
            public IReadOnlyDictionary<string, string> HttpRequestHeaders { get; internal set; }

            /// <summary>Corpo da requisição.</summary>
            public string HttpRequestBody { get; internal set; }

            /// <summary>Status devolvido, ou <c>null</c> se a chamada foi cancelada.</summary>
            public int? HttpStatus { get; internal set; }

            /// <summary>Corpo da resposta.</summary>
            public string HttpResponseBody { get; internal set; }

            /// <summary>Se a chamada foi cancelada antes de responder.</summary>
            public bool HttpCancelled { get; internal set; }

            /// <summary>Duração da chamada no simulador.</summary>
            public TimeSpan HttpElapsed { get; internal set; }
        }

        /// <summary>Sequência única e ordenada, compartilhada pelo sender e pelo cliente HTTP simulados.</summary>
        public sealed class InteractionLog
        {
            private readonly object _gate = new object();
            private readonly List<Interaction> _items = new List<Interaction>();

            /// <summary>Acrescenta um item ao fim da sequência e o devolve para ser preenchido.</summary>
            public Interaction Add(InteractionKind kind)
            {
                lock (_gate)
                {
                    var interaction = new Interaction(_items.Count + 1, kind);
                    _items.Add(interaction);
                    return interaction;
                }
            }

            /// <summary>Cópia da sequência atual.</summary>
            public IReadOnlyList<Interaction> Snapshot()
            {
                lock (_gate)
                {
                    return _items.ToArray();
                }
            }
        }

        private sealed class InMemoryContextExtension : IContextExtension
        {
            private const string GLOBAL_SCOPE = "*";

            private readonly ConcurrentDictionary<string, Document> _variables =
                new ConcurrentDictionary<string, Document>(StringComparer.OrdinalIgnoreCase);

            public string GetText(Identity identity, string variableName)
                => _variables.TryGetValue(Key(identity, variableName), out var document) ? (document as PlainText)?.Text : null;

            public Task<T> GetVariableAsync<T>(Identity identity, string variableName, CancellationToken cancellationToken)
                where T : Document
                => Task.FromResult(_variables.TryGetValue(Key(identity, variableName), out var document) ? document as T : null);

            public Task SetVariableAsync<T>(Identity identity, string variableName, T document, CancellationToken cancellationToken, TimeSpan expiration = default)
                where T : Document
            {
                _variables[Key(identity, variableName)] = document;
                return Task.CompletedTask;
            }

            public Task SetGlobalVariableAsync<T>(string variableName, T document, CancellationToken cancellationToken, TimeSpan expiration = default)
                where T : Document
            {
                _variables[Key(null, variableName)] = document;
                return Task.CompletedTask;
            }

            public Task DeleteVariableAsync(Identity identity, string variableName, CancellationToken cancellationToken)
            {
                _variables.TryRemove(Key(identity, variableName), out _);
                return Task.CompletedTask;
            }

            public Task DeleteGlobalVariableAsync(string variableName, CancellationToken cancellationToken)
            {
                _variables.TryRemove(Key(null, variableName), out _);
                return Task.CompletedTask;
            }

            public Task<DocumentCollection> GetVariablesAsync(Identity identity, int skip = 0, int take = 100, CancellationToken cancellationToken = default)
            {
                var prefix = Key(identity, string.Empty);
                var names = new List<Document>();
                foreach (var key in _variables.Keys)
                {
                    if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        names.Add(new PlainText { Text = key.Substring(prefix.Length) });
                    }
                }

                var page = names.GetRange(Math.Min(skip, names.Count), Math.Max(0, Math.Min(take, names.Count - skip)));
                return Task.FromResult(new DocumentCollection
                {
                    ItemType = PlainText.MediaType,
                    Total = names.Count,
                    Items = page.ToArray(),
                });
            }

            private static string Key(Identity identity, string variableName)
                => (identity?.ToString() ?? GLOBAL_SCOPE) + "|" + variableName;
        }
    }
}
