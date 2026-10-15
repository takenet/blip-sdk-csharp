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
    /// Runs a reference flow on the Builder engine, following <c>FlowManagerTestsBase.CreateContainer</c>: one
    /// container per scenario with <c>RegisterBuilder()</c>, simulated <see cref="ISender"/> and <see cref="IHttpClient"/>,
    /// substituted extensions and in-memory user context (no Redis and no network). Created by Reqnroll context injection,
    /// one instance per scenario, shared by the feature steps.
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

        /// <summary>Creates the harness with the simulated checkout-api in normal mode and the engine default script time.</summary>
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

        /// <summary>Kind of each item recorded in the sequence.</summary>
        public enum InteractionKind
        {
            /// <summary>Command sent or processed by the engine (<see cref="Interaction.Envelope"/> is a <see cref="Command"/>).</summary>
            Command,

            /// <summary>Message sent by the engine (<see cref="Interaction.Envelope"/> is a <see cref="Message"/>).</summary>
            Message,

            /// <summary>Notification sent by the engine.</summary>
            Notification,

            /// <summary>HTTP call made by <c>request.fetchAsync</c>.</summary>
            Http,
        }

        /// <summary>Simulated <see cref="ISender"/> registered in the container.</summary>
        public SimulatedSender Sender { get; }

        /// <summary>Simulated <see cref="IHttpClient"/> registered in the container.</summary>
        public SimulatedHttpClient HttpClient { get; }

        /// <summary>Substituted scheduler extension (input expiration).</summary>
        public ISchedulerExtension SchedulerExtension { get; }

        /// <summary>Substituted event extension (TrackEvent action).</summary>
        public IEventTrackExtension EventTrackExtension { get; }

        /// <summary>Substituted contact extension, which returns the synthetic contact.</summary>
        public IContactExtension ContactExtension { get; }

        /// <summary>Substituted engine monitoring logger.</summary>
        public IBlipLogger MonitoringLogger { get; }

        /// <summary>Maximum <c>ExecuteScriptV2</c> time; applies to the container created on the next send.</summary>
        public TimeSpan ScriptTimeout { get; set; }

        /// <summary>Reference flow in use (fixture name).</summary>
        public string ReferenceFlow { get; private set; }

        /// <summary>Block configuration in use (<c>configuracao</c> column of <c>configuracoes-v1.json</c>).</summary>
        public string ConfigurationName { get; private set; }

        /// <summary>Exception returned by the engine on the last send, or <c>null</c> if it finished without error.</summary>
        public Exception EngineFailure { get; private set; }

        /// <summary>Ordered sequence of commands, messages, notifications and HTTP calls recorded so far.</summary>
        public IReadOnlyList<Interaction> Interactions => _log.Snapshot();

        /// <summary>Bucket secret and auth-api token generated in this run, for the normalizer to mask.</summary>
        public IReadOnlyList<string> RunSecrets
            => _bucketSecret == null ? new[] { HttpClient.BotToken } : new[] { _bucketSecret, HttpClient.BotToken };

        /// <summary>Chooses the reference flow and the configuration row used on the next send.</summary>
        public void UseReferenceFlow(string referenceFlow, string configuration, bool feedback)
        {
            var row = FlowAssembler.LoadConfiguration(configuration, feedback);
            _flow = FlowAssembler.Assemble(referenceFlow, row);
            HttpClient.HasSavedCard = (bool?)row["cartaoSalvo"] ?? false;
            ReferenceFlow = referenceFlow;
            ConfigurationName = configuration;
        }

        /// <summary>Generates this run's secret, stores it in the bucket <paramref name="bucketId"/> and makes the auth-api accept it.</summary>
        public void StoreBucketSecret(string bucketId)
        {
            _bucketSecret = SyntheticData.NewRunSecret();
            Sender.SetBucketText(bucketId, _bucketSecret);
            HttpClient.AcceptedSecret = _bucketSecret;
        }

        /// <summary>Tells whether <paramref name="value"/> is this run's bucket secret, without exposing the value.</summary>
        public bool IsBucketSecret(string value)
            => _bucketSecret != null && string.Equals(value, _bucketSecret, StringComparison.Ordinal);

        /// <summary>Sends to the engine the customer message that takes the flow to the block and keeps the failure, if any.</summary>
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
        /// Delivers to the engine, without real waiting, the input expiration it scheduled in the <see cref="SchedulerExtension"/> after
        /// the last delivery, as the scheduler would at the end of the time, and keeps the failure, if any.
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

        /// <summary>Customer state in the flow recorded by the engine, or <c>null</c> if no state is recorded.</summary>
        public string UserStateId => GetContextVariable(USER_STATE_VARIABLE_PREFIX + SyntheticData.FLOW_ID);

        /// <summary>Current value of the customer context variable, or <c>null</c> if it does not exist.</summary>
        public string GetContextVariable(string name)
            => _contextExtension.GetText(Identity.Parse(SyntheticData.USER_IDENTITY), name);

        /// <summary>Serializes the envelope with the engine serializer, as it would go to the server.</summary>
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

        /// <summary>Item of the sequence recorded by the simulators.</summary>
        public sealed class Interaction
        {
            internal Interaction(int sequence, InteractionKind kind)
            {
                Sequence = sequence;
                Kind = kind;
            }

            /// <summary>Position in the sequence, starting at 1.</summary>
            public int Sequence { get; }

            /// <summary>Item kind.</summary>
            public InteractionKind Kind { get; }

            /// <summary>Envelope sent by the engine (command, message or notification).</summary>
            public Envelope Envelope { get; internal set; }

            /// <summary>HTTP method.</summary>
            public string HttpMethod { get; internal set; }

            /// <summary>Called URL.</summary>
            public Uri HttpUri { get; internal set; }

            /// <summary>Request headers.</summary>
            public IReadOnlyDictionary<string, string> HttpRequestHeaders { get; internal set; }

            /// <summary>Request body.</summary>
            public string HttpRequestBody { get; internal set; }

            /// <summary>Returned status, or <c>null</c> if the call was cancelled.</summary>
            public int? HttpStatus { get; internal set; }

            /// <summary>Response body.</summary>
            public string HttpResponseBody { get; internal set; }

            /// <summary>Whether the call was cancelled before responding.</summary>
            public bool HttpCancelled { get; internal set; }

            /// <summary>Call duration in the simulator.</summary>
            public TimeSpan HttpElapsed { get; internal set; }
        }

        /// <summary>Single ordered sequence, shared by the simulated sender and HTTP client.</summary>
        public sealed class InteractionLog
        {
            private readonly object _gate = new object();
            private readonly List<Interaction> _items = new List<Interaction>();

            /// <summary>Appends an item to the end of the sequence and returns it to be filled.</summary>
            public Interaction Add(InteractionKind kind)
            {
                lock (_gate)
                {
                    var interaction = new Interaction(_items.Count + 1, kind);
                    _items.Add(interaction);
                    return interaction;
                }
            }

            /// <summary>Copy of the current sequence.</summary>
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
