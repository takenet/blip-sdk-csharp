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
    /// Steps of <c>execucao_do_bloco.feature</c>. The step that sends the customer message is shared with
    /// <c>mensagens_do_bloco_v1.feature</c> through the scenario <see cref="BuilderEngineHarness"/>.
    /// </summary>
    [Binding]
    public sealed class BlockExecutionSteps
    {
        /// <summary>
        /// Block configuration used by <c>execucao_do_bloco.feature</c>: payment link and PIX through Checkout Blip,
        /// the two payment methods that the Checkout contract enables.
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

        /// <summary>Receives the scenario harness through Reqnroll context injection.</summary>
        public BlockExecutionSteps(BuilderEngineHarness harness)
        {
            _harness = harness ?? throw new ArgumentNullException(nameof(harness));
        }

        /// <summary>Loads the reference flow with the <see cref="BLOCK_CONFIGURATION"/> configuration.</summary>
        [Given("o fluxo de referência {string} carregado no motor do Builder")]
        public void GivenReferenceFlowLoaded(string referenceFlow)
        {
            _harness.UseReferenceFlow(referenceFlow, BLOCK_CONFIGURATION, false);
        }

        /// <summary>Stores in the bucket a secret generated in this run.</summary>
        [Given("o bucket {string} do bot com um segredo de teste")]
        public void GivenBucketWithTestSecret(string bucketId)
        {
            _bucketId = bucketId;
            _harness.StoreBucketSecret(bucketId);
        }

        /// <summary>Puts the simulated checkout-auth-api and checkout-api in normal mode.</summary>
        [Given("a checkout-auth-api e a checkout-api simuladas respondendo normalmente")]
        public void GivenApisRespondingNormally()
        {
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Normal;
        }

        /// <summary>Makes the simulated checkout-api respond with an error when creating the link.</summary>
        [Given("que a checkout-api simulada responde erro na criação do link")]
        public void GivenCheckoutApiFailsCreatingLink()
        {
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Error;
        }

        /// <summary>Shortens the maximum script time and makes the checkout-api take longer than it.</summary>
        [Given("que a checkout-api simulada demora mais que o tempo máximo de script configurado")]
        public void GivenCheckoutApiSlowerThanScriptTimeout()
        {
            _harness.ScriptTimeout = SHORT_SCRIPT_TIMEOUT;
            _harness.HttpClient.CheckoutApiDelay = SHORT_SCRIPT_TIMEOUT + DELAY_BEYOND_SCRIPT_TIMEOUT;
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Delay;
        }

        /// <summary>Sends to the engine the customer message that leads to the block (shared between the features).</summary>
        [When("o cliente envia a mensagem que leva ao bloco de Pagamentos")]
        public Task WhenClientSendsMessageToPaymentBlock()
        {
            return _harness.SendClientMessageAsync();
        }

        /// <summary>Checks the bucket read through a command with <c>#envelope.sendToServerRequired=true</c>, before any HTTP call.</summary>
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

            reads.Count.ShouldBe(1, "reads of bucket " + bucketId + ". Sequence: " + Describe(interactions));
            var metadata = ((Command)reads[0].Envelope).Metadata;
            metadata.ShouldNotBeNull("the bucket command was sent without metadata");
            metadata.TryGetValue(SEND_TO_SERVER_METADATA, out var sendToServer).ShouldBeTrue("the bucket command was sent without " + SEND_TO_SERVER_METADATA);
            sendToServer.ShouldBe("true");
            FirstHttp(interactions).ShouldBeGreaterThan(reads[0].Sequence, "the bucket should be read before any HTTP call");
        }

        /// <summary>Checks the call to the checkout-auth-api with the bucket secret, after the bucket read.</summary>
        [Then("chama a checkout-auth-api para obter o token do bot")]
        public void ThenCallsAuthApiForBotToken()
        {
            var interactions = AssertEngineCompleted();
            var calls = HttpCalls(interactions, SyntheticData.AUTH_HOST, SimulatedHttpClient.AUTH_TOKEN_PATH, POST);
            calls.Count.ShouldBeGreaterThan(0, "no call to checkout-auth-api. Sequence: " + Describe(interactions));
            var first = calls[0];
            first.Sequence.ShouldBeGreaterThan(BucketReadSequence(interactions), "checkout-auth-api was called before the bucket was read");
            _harness.IsBucketSecret((string)JObject.Parse(first.HttpRequestBody)["secret"])
                .ShouldBeTrue("checkout-auth-api did not receive the secret read from the bucket");
            first.HttpStatus.ShouldBe(200, "the simulated checkout-auth-api did not issue the token");
        }

        /// <summary>Checks the link creation in the checkout-api with the issued token, after the checkout-auth-api.</summary>
        [Then("chama a checkout-api para criar o link de pagamento")]
        public void ThenCallsCheckoutApiToCreateLink()
        {
            var interactions = AssertEngineCompleted();
            var purchases = HttpCalls(interactions, SyntheticData.CHECKOUT_HOST, SimulatedHttpClient.PURCHASE_PATH, POST);
            purchases.Count.ShouldBe(1, "link creations on checkout-api. Sequence: " + Describe(interactions));
            var purchase = purchases[0];
            purchase.Sequence.ShouldBeGreaterThan(FirstAuthSequence(interactions), "checkout-api was called before checkout-auth-api");
            purchase.HttpRequestHeaders.TryGetValue("Authorization", out var authorization).ShouldBeTrue("checkout-api was called without Authorization");
            string.Equals(authorization, BEARER_PREFIX + _harness.HttpClient.BotToken, StringComparison.Ordinal)
                .ShouldBeTrue("checkout-api did not receive the token issued by checkout-auth-api");
            purchase.HttpStatus.ShouldBe(200);
        }

        /// <summary>Checks a single order card, sent to the customer after the link creation and with the created link.</summary>
        [Then("envia ao cliente o cartão de pedido com o link")]
        public void ThenSendsOrderCardWithLink()
        {
            var interactions = AssertEngineCompleted();
            var cards = OrderCards(interactions);
            cards.Count.ShouldBe(1, "order cards sent. Sequence: " + Describe(interactions));
            var (interaction, content) = cards[0];
            interaction.Sequence.ShouldBeGreaterThan(PurchaseSequence(interactions), "the card was sent before the link was created");
            interaction.Envelope.To.ToIdentity().ShouldBe(Identity.Parse(SyntheticData.USER_IDENTITY));
            PaymentLinks(content).ShouldBe(new[] { SyntheticData.PAYMENT_LINK });
        }

        /// <summary>Checks that no order card was sent.</summary>
        [Then("nenhum cartão de pedido é enviado ao cliente")]
        public void ThenNoOrderCardIsSent()
        {
            var interactions = _harness.Interactions;
            OrderCards(interactions).Count.ShouldBe(0, "order cards sent. Sequence: " + Describe(interactions));
        }

        /// <summary>
        /// Checks that the link creation failure is recorded: the engine takes the customer to the block error output and records,
        /// through the TrackEvent action, the payment event with <c>error_status</c> equal to <c>Error</c>.
        /// </summary>
        [Then("a falha fica registrada pelo motor")]
        public void ThenFailureIsRecordedByEngine()
        {
            var interactions = AssertEngineCompleted();
            var purchases = HttpCalls(interactions, SyntheticData.CHECKOUT_HOST, SimulatedHttpClient.PURCHASE_PATH, POST);
            purchases.Count.ShouldBeGreaterThan(0, "no link creation on checkout-api. Sequence: " + Describe(interactions));
            foreach (var purchase in purchases)
            {
                purchase.HttpStatus.ShouldBe(500, "the simulated checkout-api should fail to create the link");
            }

            _harness.UserStateId.ShouldBe(FlowAssembler.ExitStateId(ERROR_OUTPUT), "the engine did not take the client to the block error output");
            PaymentEventErrorStatus().ShouldBe(ERROR_OUTPUT, "the engine did not track the payment event with the failure");
        }

        /// <summary>
        /// Checks that the link creation, pending beyond the maximum script time, was cancelled by that limit: the call
        /// did not respond and lasted less than the limit plus the tolerance. The tolerance absorbs a loaded CI agent and stays
        /// well below the simulated delay, so a cancellation that does not come from the limit still fails.
        /// </summary>
        [Then("a execução do script é interrompida por tempo")]
        public void ThenScriptIsInterruptedByTimeout()
        {
            var interactions = _harness.Interactions;
            var purchases = HttpCalls(interactions, SyntheticData.CHECKOUT_HOST, SimulatedHttpClient.PURCHASE_PATH, POST);
            purchases.Count.ShouldBe(1, "link creations on checkout-api. Sequence: " + Describe(interactions));
            var purchase = purchases[0];
            var limit = _harness.ScriptTimeout + TIMEOUT_TOLERANCE;
            limit.ShouldBeLessThan(_harness.HttpClient.CheckoutApiDelay, "the tolerance must stay below the simulated delay");
            purchase.HttpCancelled.ShouldBeTrue("the link creation responded instead of being interrupted. Sequence: " + Describe(interactions));
            purchase.HttpElapsed.ShouldBeLessThan(limit, "the link creation was not interrupted by the script timeout");
        }

        private IReadOnlyList<BuilderEngineHarness.Interaction> AssertEngineCompleted()
        {
            _harness.EngineFailure.ShouldBeNull("the engine finished with " + _harness.EngineFailure?.GetType().Name);
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

            throw new ShouldAssertException("bucket " + _bucketId + " was not read");
        }

        private static int FirstAuthSequence(IReadOnlyList<BuilderEngineHarness.Interaction> interactions)
        {
            var calls = HttpCalls(interactions, SyntheticData.AUTH_HOST, SimulatedHttpClient.AUTH_TOKEN_PATH, POST);
            calls.Count.ShouldBeGreaterThan(0, "no call to checkout-auth-api");
            return calls[0].Sequence;
        }

        private static int PurchaseSequence(IReadOnlyList<BuilderEngineHarness.Interaction> interactions)
        {
            var calls = HttpCalls(interactions, SyntheticData.CHECKOUT_HOST, SimulatedHttpClient.PURCHASE_PATH, POST);
            calls.Count.ShouldBeGreaterThan(0, "no link creation on checkout-api");
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
                        .Append(" -> ").Append(interaction.HttpCancelled ? "cancelled" : interaction.HttpStatus?.ToString());
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
