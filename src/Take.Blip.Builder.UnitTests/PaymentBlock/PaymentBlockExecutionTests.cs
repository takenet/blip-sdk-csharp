using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Shouldly;
using Take.Blip.Builder.UnitTests.PaymentBlock.Support;
using Xunit;

namespace Take.Blip.Builder.UnitTests.PaymentBlock
{
    /// <summary>
    /// Runs the Payments block v1 (approved reference <c>checkout-v1</c>, exported by the portal-submodule-builder suite) on the
    /// same engine used by Template Hosting, with commands and HTTP calls simulated. Protects v1 while it exists.
    /// Each test is one scenario: the arrange step loads the flow, stores the bucket secret and puts the simulated APIs in normal mode.
    /// </summary>
    [Trait(CATEGORY, REGRESSION)]
    [Trait(CATEGORY, PAYMENT_BLOCK)]
    public sealed class PaymentBlockExecutionTests : IDisposable
    {
        private const string CATEGORY = "Category";

        // These two traits mirror the Gherkin tags of the IPAY-1 decision, shared by every Payments repository
        // (--filter Category=regressao), so they keep their original names.
        private const string REGRESSION = "regressao";
        private const string PAYMENT_BLOCK = "bloco-pagamento";

        private const string REFERENCE_FLOW = "checkout-v1";

        // Payment link and PIX through Checkout Blip, the two payment methods that the Checkout contract enables.
        private const string BLOCK_CONFIGURATION = "checkout-link-and-pix";
        private const string BUCKET_ID = "checkout-secret";
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

        private readonly BuilderEngineHarness _harness = new BuilderEngineHarness();

        public void Dispose()
        {
            _harness.Dispose();
        }

        /// <summary>
        /// Entering the block sends the order card with the created link. Scenario "Cliente entra no bloco e recebe o pedido com o link"
        /// of the Payments regression suite.
        /// </summary>
        [Fact]
        public async Task ClientEntersBlockAndReceivesOrderWithLink()
        {
            // Arrange
            GivenReferenceFlowWithBucketSecretAndApisRespondingNormally();

            // Act
            await _harness.SendClientMessageAsync();

            // Assert
            ThenEngineReadsBucketThroughIrisServer();
            ThenCallsAuthApiForBotToken();
            ThenCallsCheckoutApiToCreateLink();
            ThenSendsOrderCardWithLink();
        }

        /// <summary>
        /// A checkout-api failure sends no link and the engine records the failure. Scenario "Falha da checkout-api não envia link"
        /// of the Payments regression suite.
        /// </summary>
        [Fact]
        public async Task CheckoutApiFailureShouldNotSendLink()
        {
            // Arrange
            GivenReferenceFlowWithBucketSecretAndApisRespondingNormally();
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Error;

            // Act
            await _harness.SendClientMessageAsync();

            // Assert
            ThenNoOrderCardIsSent();
            ThenFailureIsRecordedByEngine();
        }

        /// <summary>
        /// The block script is interrupted at the maximum execution time and sends no order card. Scenario
        /// "Script do bloco respeita o tempo máximo de execução" of the Payments regression suite.
        /// </summary>
        [Fact]
        public async Task BlockScriptShouldRespectMaximumExecutionTime()
        {
            // Arrange
            GivenReferenceFlowWithBucketSecretAndApisRespondingNormally();
            _harness.ScriptTimeout = SHORT_SCRIPT_TIMEOUT;
            _harness.HttpClient.CheckoutApiDelay = SHORT_SCRIPT_TIMEOUT + DELAY_BEYOND_SCRIPT_TIMEOUT;
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Delay;

            // Act
            await _harness.SendClientMessageAsync();

            // Assert
            ThenScriptIsInterruptedByTimeout();
            ThenNoOrderCardIsSent();
        }

        private void GivenReferenceFlowWithBucketSecretAndApisRespondingNormally()
        {
            _harness.UseReferenceFlow(REFERENCE_FLOW, BLOCK_CONFIGURATION, false);
            _harness.StoreBucketSecret(BUCKET_ID);
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Normal;
        }

        /// <summary>Checks the bucket read through a command with <c>#envelope.sendToServerRequired=true</c>, before any HTTP call.</summary>
        private void ThenEngineReadsBucketThroughIrisServer()
        {
            var interactions = AssertEngineCompleted();
            var reads = new List<BuilderEngineHarness.Interaction>();
            foreach (var interaction in interactions)
            {
                if (interaction.Envelope is Command command
                    && command.Method == CommandMethod.Get
                    && string.Equals(command.Uri?.ToString(), BUCKET_URI_PREFIX + BUCKET_ID, StringComparison.OrdinalIgnoreCase))
                {
                    reads.Add(interaction);
                }
            }

            reads.Count.ShouldBe(1, "reads of bucket " + BUCKET_ID + ". Sequence: " + Describe(interactions));
            var metadata = ((Command)reads[0].Envelope).Metadata;
            metadata.ShouldNotBeNull("the bucket command was sent without metadata");
            metadata.TryGetValue(SEND_TO_SERVER_METADATA, out var sendToServer).ShouldBeTrue("the bucket command was sent without " + SEND_TO_SERVER_METADATA);
            sendToServer.ShouldBe("true");
            FirstHttp(interactions).ShouldBeGreaterThan(reads[0].Sequence, "the bucket should be read before any HTTP call");
        }

        /// <summary>Checks the call to the checkout-auth-api with the bucket secret, after the bucket read.</summary>
        private void ThenCallsAuthApiForBotToken()
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
        private void ThenCallsCheckoutApiToCreateLink()
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
        private void ThenSendsOrderCardWithLink()
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
        private void ThenNoOrderCardIsSent()
        {
            var interactions = _harness.Interactions;
            OrderCards(interactions).Count.ShouldBe(0, "order cards sent. Sequence: " + Describe(interactions));
        }

        /// <summary>
        /// Checks that the link creation failure is recorded: the engine takes the customer to the block error output and records,
        /// through the TrackEvent action, the payment event with <c>error_status</c> equal to <c>Error</c>.
        /// </summary>
        private void ThenFailureIsRecordedByEngine()
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
        private void ThenScriptIsInterruptedByTimeout()
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

        private static int BucketReadSequence(IReadOnlyList<BuilderEngineHarness.Interaction> interactions)
        {
            foreach (var interaction in interactions)
            {
                if (interaction.Envelope is Command command
                    && string.Equals(command.Uri?.ToString(), BUCKET_URI_PREFIX + BUCKET_ID, StringComparison.OrdinalIgnoreCase))
                {
                    return interaction.Sequence;
                }
            }

            throw new ShouldAssertException("bucket " + BUCKET_ID + " was not read");
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
