using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Reqnroll;
using Shouldly;
using Take.Blip.Builder.PaymentBlock.Tests.Support;

namespace Take.Blip.Builder.PaymentBlock.Tests.Steps
{
    /// <summary>
    /// Passos de <c>execucao_do_bloco.feature</c>. O passo de envio da mensagem do cliente é compartilhado com
    /// <c>mensagens_do_bloco_v1.feature</c> pelo <see cref="BuilderEngineHarness"/> do cenário.
    /// </summary>
    [Binding]
    public sealed class BlockExecutionSteps
    {
        /// <summary>
        /// Configuração do bloco usada por <c>execucao_do_bloco.feature</c>: link de pagamento e PIX pelo Checkout Blip,
        /// os dois meios que a contratação do Checkout habilita.
        /// </summary>
        public const string BLOCK_CONFIGURATION = "checkout-link-e-pix";

        private const string BUCKET_URI_PREFIX = "/buckets/";
        private const string SEND_TO_SERVER_METADATA = "#envelope.sendToServerRequired";
        private const string ORDER_CARD_TYPE = "order_details";
        private const string BEARER_PREFIX = "Bearer ";
        private const string POST = "POST";
        private const string ERROR_OUTPUT = "Error";
        private const string PAYMENT_EVENT_CATEGORY = "payment_request";
        private const string PAYMENT_EVENT_ERROR_STATUS = "error_status";
        private static readonly TimeSpan SHORT_SCRIPT_TIMEOUT = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan DELAY_BEYOND_SCRIPT_TIMEOUT = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan TIMEOUT_TOLERANCE = TimeSpan.FromSeconds(10);

        private readonly BuilderEngineHarness _harness;
        private string _bucketId;

        /// <summary>Recebe o harness do cenário pela injeção de contexto do Reqnroll.</summary>
        public BlockExecutionSteps(BuilderEngineHarness harness)
        {
            _harness = harness ?? throw new ArgumentNullException(nameof(harness));
        }

        /// <summary>Carrega o fluxo de referência com a configuração <see cref="BLOCK_CONFIGURATION"/>.</summary>
        [Given("o fluxo de referência {string} carregado no motor do Builder")]
        public void GivenReferenceFlowLoaded(string referenceFlow)
        {
            _harness.UseReferenceFlow(referenceFlow, BLOCK_CONFIGURATION, false);
        }

        /// <summary>Guarda no bucket um segredo gerado nesta execução.</summary>
        [Given("o bucket {string} do bot com um segredo de teste")]
        public void GivenBucketWithTestSecret(string bucketId)
        {
            _bucketId = bucketId;
            _harness.StoreBucketSecret(bucketId);
        }

        /// <summary>Deixa a checkout-auth-api e a checkout-api simuladas no modo normal.</summary>
        [Given("a checkout-auth-api e a checkout-api simuladas respondendo normalmente")]
        public void GivenApisRespondingNormally()
        {
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Normal;
        }

        /// <summary>Faz a checkout-api simulada responder erro na criação do link.</summary>
        [Given("que a checkout-api simulada responde erro na criação do link")]
        public void GivenCheckoutApiFailsCreatingLink()
        {
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Error;
        }

        /// <summary>Encurta o tempo máximo de script e faz a checkout-api demorar além dele.</summary>
        [Given("que a checkout-api simulada demora mais que o tempo máximo de script configurado")]
        public void GivenCheckoutApiSlowerThanScriptTimeout()
        {
            _harness.ScriptTimeout = SHORT_SCRIPT_TIMEOUT;
            _harness.HttpClient.CheckoutApiDelay = SHORT_SCRIPT_TIMEOUT + DELAY_BEYOND_SCRIPT_TIMEOUT;
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Delay;
        }

        /// <summary>Envia ao motor a mensagem do cliente que leva ao bloco (compartilhado entre as features).</summary>
        [When("o cliente envia a mensagem que leva ao bloco de Pagamentos")]
        public Task WhenClientSendsMessageToPaymentBlock()
        {
            return _harness.SendClientMessageAsync();
        }

        /// <summary>Confere a leitura do bucket por um comando com <c>#envelope.sendToServerRequired=true</c>, antes de qualquer HTTP.</summary>
        [Then("o motor lê o bucket {string} por um comando marcado para passar pelo Iris Server")]
        public void ThenEngineReadsBucketThroughIrisServer(string bucketId)
        {
            var interactions = AssertEngineCompleted();
            var reads = new List<BuilderEngineHarness.Interaction>();
            foreach (var interaction in interactions)
            {
                if (interaction.Envelope is Command command
                    && command.Method == CommandMethod.Get
                    && string.Equals(command.Uri?.ToString(), BUCKET_URI_PREFIX + bucketId, StringComparison.OrdinalIgnoreCase))
                {
                    reads.Add(interaction);
                }
            }

            reads.Count.ShouldBe(1, "leituras do bucket " + bucketId + ". Sequência: " + Describe(interactions));
            var metadata = ((Command)reads[0].Envelope).Metadata;
            metadata.ShouldNotBeNull("o comando do bucket saiu sem metadados");
            metadata.TryGetValue(SEND_TO_SERVER_METADATA, out var sendToServer).ShouldBeTrue("o comando do bucket saiu sem " + SEND_TO_SERVER_METADATA);
            sendToServer.ShouldBe("true");
            FirstHttp(interactions).ShouldBeGreaterThan(reads[0].Sequence, "o bucket deveria ser lido antes de qualquer chamada HTTP");
        }

        /// <summary>Confere a chamada à checkout-auth-api com o segredo do bucket, depois da leitura do bucket.</summary>
        [Then("chama a checkout-auth-api para obter o token do bot")]
        public void ThenCallsAuthApiForBotToken()
        {
            var interactions = AssertEngineCompleted();
            var calls = HttpCalls(interactions, SyntheticData.AUTH_HOST, SimulatedHttpClient.AUTH_TOKEN_PATH, POST);
            calls.Count.ShouldBeGreaterThan(0, "nenhuma chamada à checkout-auth-api. Sequência: " + Describe(interactions));
            var first = calls[0];
            first.Sequence.ShouldBeGreaterThan(BucketReadSequence(interactions), "a checkout-auth-api foi chamada antes da leitura do bucket");
            _harness.IsBucketSecret((string)JObject.Parse(first.HttpRequestBody)["secret"])
                .ShouldBeTrue("a checkout-auth-api não recebeu o segredo lido do bucket");
            first.HttpStatus.ShouldBe(200, "a checkout-auth-api simulada não emitiu o token");
        }

        /// <summary>Confere a criação do link na checkout-api com o token emitido, depois da checkout-auth-api.</summary>
        [Then("chama a checkout-api para criar o link de pagamento")]
        public void ThenCallsCheckoutApiToCreateLink()
        {
            var interactions = AssertEngineCompleted();
            var purchases = HttpCalls(interactions, SyntheticData.CHECKOUT_HOST, SimulatedHttpClient.PURCHASE_PATH, POST);
            purchases.Count.ShouldBe(1, "criações de link na checkout-api. Sequência: " + Describe(interactions));
            var purchase = purchases[0];
            purchase.Sequence.ShouldBeGreaterThan(FirstAuthSequence(interactions), "a checkout-api foi chamada antes da checkout-auth-api");
            purchase.HttpRequestHeaders.TryGetValue("Authorization", out var authorization).ShouldBeTrue("a checkout-api foi chamada sem Authorization");
            string.Equals(authorization, BEARER_PREFIX + _harness.HttpClient.BotToken, StringComparison.Ordinal)
                .ShouldBeTrue("a checkout-api não recebeu o token emitido pela checkout-auth-api");
            purchase.HttpStatus.ShouldBe(200);
        }

        /// <summary>Confere um único cartão de pedido, enviado ao cliente depois da criação do link e com o link criado.</summary>
        [Then("envia ao cliente o cartão de pedido com o link")]
        public void ThenSendsOrderCardWithLink()
        {
            var interactions = AssertEngineCompleted();
            var cards = OrderCards(interactions);
            cards.Count.ShouldBe(1, "cartões de pedido enviados. Sequência: " + Describe(interactions));
            var (interaction, content) = cards[0];
            interaction.Sequence.ShouldBeGreaterThan(PurchaseSequence(interactions), "o cartão saiu antes da criação do link");
            interaction.Envelope.To.ToIdentity().ShouldBe(Identity.Parse(SyntheticData.USER_IDENTITY));
            PaymentLinks(content).ShouldBe(new[] { SyntheticData.PAYMENT_LINK });
        }

        /// <summary>Confere que nenhum cartão de pedido foi enviado.</summary>
        [Then("nenhum cartão de pedido é enviado ao cliente")]
        public void ThenNoOrderCardIsSent()
        {
            var interactions = _harness.Interactions;
            OrderCards(interactions).Count.ShouldBe(0, "cartões de pedido enviados. Sequência: " + Describe(interactions));
        }

        /// <summary>
        /// Confere o registro da falha da criação do link: o motor leva o cliente à saída de erro do bloco e grava, pela
        /// ação TrackEvent, o evento de pagamento com <c>error_status</c> igual a <c>Error</c>.
        /// </summary>
        [Then("a falha fica registrada pelo motor")]
        public void ThenFailureIsRecordedByEngine()
        {
            var interactions = AssertEngineCompleted();
            var purchases = HttpCalls(interactions, SyntheticData.CHECKOUT_HOST, SimulatedHttpClient.PURCHASE_PATH, POST);
            purchases.Count.ShouldBeGreaterThan(0, "nenhuma criação de link na checkout-api. Sequência: " + Describe(interactions));
            foreach (var purchase in purchases)
            {
                purchase.HttpStatus.ShouldBe(500, "a checkout-api simulada deveria falhar na criação do link");
            }

            _harness.UserStateId.ShouldBe(FlowAssembler.ExitStateId(ERROR_OUTPUT), "o motor não levou o cliente à saída de erro do bloco");
            PaymentEventErrorStatus().ShouldBe(ERROR_OUTPUT, "o motor não registrou o evento de pagamento com a falha");
        }

        /// <summary>
        /// Confere que a criação do link, pendente além do tempo máximo de script, foi cancelada por esse limite: a chamada
        /// não respondeu e durou menos que o limite mais a tolerância. A tolerância absorve o agente de CI sob carga e fica
        /// bem abaixo da demora simulada, então um cancelamento que não venha do limite ainda reprova.
        /// </summary>
        [Then("a execução do script é interrompida por tempo")]
        public void ThenScriptIsInterruptedByTimeout()
        {
            var interactions = _harness.Interactions;
            var purchases = HttpCalls(interactions, SyntheticData.CHECKOUT_HOST, SimulatedHttpClient.PURCHASE_PATH, POST);
            purchases.Count.ShouldBe(1, "criações de link na checkout-api. Sequência: " + Describe(interactions));
            var purchase = purchases[0];
            var limit = _harness.ScriptTimeout + TIMEOUT_TOLERANCE;
            limit.ShouldBeLessThan(_harness.HttpClient.CheckoutApiDelay, "a tolerância precisa ficar abaixo da demora simulada");
            purchase.HttpCancelled.ShouldBeTrue("a criação do link respondeu em vez de ser interrompida. Sequência: " + Describe(interactions));
            purchase.HttpElapsed.ShouldBeLessThan(limit, "a criação do link não foi interrompida pelo tempo máximo de script");
        }

        private IReadOnlyList<BuilderEngineHarness.Interaction> AssertEngineCompleted()
        {
            _harness.EngineFailure.ShouldBeNull("o motor terminou com " + _harness.EngineFailure?.GetType().Name);
            return _harness.Interactions;
        }

        private int BucketReadSequence(IReadOnlyList<BuilderEngineHarness.Interaction> interactions)
        {
            foreach (var interaction in interactions)
            {
                if (interaction.Envelope is Command command
                    && string.Equals(command.Uri?.ToString(), BUCKET_URI_PREFIX + _bucketId, StringComparison.OrdinalIgnoreCase))
                {
                    return interaction.Sequence;
                }
            }

            throw new ShouldAssertException("o bucket " + _bucketId + " não foi lido");
        }

        private static int FirstAuthSequence(IReadOnlyList<BuilderEngineHarness.Interaction> interactions)
        {
            var calls = HttpCalls(interactions, SyntheticData.AUTH_HOST, SimulatedHttpClient.AUTH_TOKEN_PATH, POST);
            calls.Count.ShouldBeGreaterThan(0, "nenhuma chamada à checkout-auth-api");
            return calls[0].Sequence;
        }

        private static int PurchaseSequence(IReadOnlyList<BuilderEngineHarness.Interaction> interactions)
        {
            var calls = HttpCalls(interactions, SyntheticData.CHECKOUT_HOST, SimulatedHttpClient.PURCHASE_PATH, POST);
            calls.Count.ShouldBeGreaterThan(0, "nenhuma criação de link na checkout-api");
            return calls[0].Sequence;
        }

        private static int FirstHttp(IReadOnlyList<BuilderEngineHarness.Interaction> interactions)
        {
            foreach (var interaction in interactions)
            {
                if (interaction.Kind == BuilderEngineHarness.InteractionKind.Http)
                {
                    return interaction.Sequence;
                }
            }

            return int.MaxValue;
        }

        private static List<BuilderEngineHarness.Interaction> HttpCalls(
            IReadOnlyList<BuilderEngineHarness.Interaction> interactions, string host, string path, string method)
        {
            var calls = new List<BuilderEngineHarness.Interaction>();
            foreach (var interaction in interactions)
            {
                if (interaction.Kind == BuilderEngineHarness.InteractionKind.Http
                    && string.Equals(interaction.HttpMethod, method, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(interaction.HttpUri.Host, host, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(interaction.HttpUri.AbsolutePath, path, StringComparison.OrdinalIgnoreCase))
                {
                    calls.Add(interaction);
                }
            }

            return calls;
        }

        private List<(BuilderEngineHarness.Interaction Interaction, JToken Content)> OrderCards(
            IReadOnlyList<BuilderEngineHarness.Interaction> interactions)
        {
            var cards = new List<(BuilderEngineHarness.Interaction, JToken)>();
            foreach (var interaction in interactions)
            {
                if (interaction.Kind != BuilderEngineHarness.InteractionKind.Message)
                {
                    continue;
                }

                var content = _harness.ToJson(interaction.Envelope)["content"];
                if (content is JObject card
                    && string.Equals((string)card.SelectToken("interactive.type"), ORDER_CARD_TYPE, StringComparison.Ordinal))
                {
                    cards.Add((interaction, card));
                }
            }

            return cards;
        }

        private static List<string> PaymentLinks(JToken orderCard)
        {
            var links = new List<string>();
            foreach (var setting in orderCard.SelectTokens("interactive.action.parameters.payment_settings[*].payment_link.uri"))
            {
                links.Add((string)setting);
            }

            return links;
        }

        private string PaymentEventErrorStatus()
        {
            foreach (var call in _harness.EventTrackExtension.ReceivedCalls())
            {
                var arguments = call.GetArguments();
                if (arguments.Length == 0
                    || !string.Equals(arguments[0] as string, PAYMENT_EVENT_CATEGORY, StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (var argument in arguments)
                {
                    if (argument is IDictionary<string, string> extras
                        && extras.TryGetValue(PAYMENT_EVENT_ERROR_STATUS, out var status))
                    {
                        return status;
                    }
                }
            }

            return null;
        }

        private static string Describe(IReadOnlyList<BuilderEngineHarness.Interaction> interactions)
        {
            var text = new StringBuilder(interactions.Count * 48);
            foreach (var interaction in interactions)
            {
                text.Append('[').Append(interaction.Sequence).Append(' ').Append(interaction.Kind);
                if (interaction.Kind == BuilderEngineHarness.InteractionKind.Http)
                {
                    text.Append(' ').Append(interaction.HttpMethod)
                        .Append(' ').Append(interaction.HttpUri.Host).Append(interaction.HttpUri.AbsolutePath)
                        .Append(" -> ").Append(interaction.HttpCancelled ? "cancelada" : interaction.HttpStatus?.ToString());
                }
                else if (interaction.Envelope is Command command)
                {
                    text.Append(' ').Append(command.Method).Append(' ').Append(command.Uri);
                }

                text.Append("] ");
            }

            return text.ToString();
        }
    }
}
