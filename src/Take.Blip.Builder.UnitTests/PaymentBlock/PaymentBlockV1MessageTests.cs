using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Shouldly;
using Take.Blip.Builder.UnitTests.PaymentBlock.Comparison;
using Take.Blip.Builder.UnitTests.PaymentBlock.Support;
using Xunit;

namespace Take.Blip.Builder.UnitTests.PaymentBlock
{
    /// <summary>
    /// Runs the Payments block v1 on the Builder engine, with the checkout-api and the checkout-auth-api simulated, and compares
    /// the message sent to the customer with the approved reference in <c>PaymentBlock/References/</c>, through the
    /// <see cref="ReferenceComparer"/>. The references are used by the payment-application to prove it generates the same messages
    /// when the block logic leaves the front end.
    /// </summary>
    [Trait(CATEGORY, REGRESSION)]
    [Trait(CATEGORY, PAYMENT_BLOCK)]
    [Trait(CATEGORY, V1_REFERENCE)]
    public sealed class PaymentBlockV1MessageTests : IDisposable
    {
        private const string CATEGORY = "Category";

        // These three traits mirror the Gherkin tags of the IPAY-1 decision, shared by every Payments repository
        // (--filter Category=regressao), so they keep their original names.
        private const string REGRESSION = "regressao";
        private const string PAYMENT_BLOCK = "bloco-pagamento";
        private const string V1_REFERENCE = "referencia-v1";

        private const string REFERENCE_FLOW = "checkout-v1";
        private const string FEEDBACK_CONFIGURATION = "checkout-link-and-pix";
        private const string BUCKET_ID = "checkout-secret";

        private readonly BuilderEngineHarness _harness = new BuilderEngineHarness();

        public void Dispose()
        {
            _harness.Dispose();
        }

        /// <summary>
        /// The order message matches the approved reference for each block configuration. Scenario outline
        /// "Mensagem do pedido para cada configuração do bloco" of the Payments regression suite.
        /// </summary>
        [Theory]
        [InlineData("checkout-link", "v1-checkout-link")]
        [InlineData("checkout-pix", "v1-checkout-pix")]
        [InlineData("checkout-link-and-pix", "v1-checkout-link-and-pix")]
        [InlineData("checkout-link-with-saved-card", "v1-checkout-saved-card-cta")]
        [InlineData("own-pix-cpf", "v1-own-pix-cpf")]
        [InlineData("own-pix-random-key", "v1-own-pix-random-key")]
        [InlineData("own-link", "v1-own-link")]
        [InlineData("own-boleto", "v1-own-boleto")]
        [InlineData("physical-product-two-items", "v1-physical-two-items")]
        public async Task OrderMessageShouldMatchApprovedReference(string configuration, string reference)
        {
            // Arrange
            _harness.UseReferenceFlow(REFERENCE_FLOW, configuration, false);
            _harness.StoreBucketSecret(BUCKET_ID);
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Normal;

            // Act
            await _harness.SendClientMessageAsync();

            // Assert
            AssertEngineCompleted();
            AssertSingleMessageMatches(reference, ClientMessages(0));
        }

        /// <summary>
        /// The status message matches the approved reference for each payment result. Scenario outline
        /// "Mensagem de status para cada resultado do pagamento" of the Payments regression suite. The customer reaches the block
        /// and the engine receives, without real waiting, the input expiration it scheduled: the end of the feedback time, which
        /// triggers the payment query.
        /// </summary>
        [Theory]
        [InlineData("PIX", "Succeeded", "v1-status-pix-paid")]
        [InlineData("CreditCard", "Succeeded", "v1-status-card-paid")]
        [InlineData("CreditCard", "Denied", "v1-status-denied")]
        [InlineData("PIX", "Pending", "v1-status-pending")]
        [InlineData("PIX", "Created", "v1-status-created")]
        public async Task StatusMessageShouldMatchApprovedReference(string paymentMethod, string status, string reference)
        {
            // Arrange
            _harness.UseReferenceFlow(REFERENCE_FLOW, FEEDBACK_CONFIGURATION, true);
            _harness.StoreBucketSecret(BUCKET_ID);
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Normal;
            _harness.HttpClient.ReportPayment(paymentMethod, status);

            // Act
            await _harness.SendClientMessageAsync();
            AssertEngineCompleted();
            var feedbackStartSequence = _harness.Interactions.Count;
            await _harness.DeliverScheduledInputExpirationAsync();

            // Assert
            AssertEngineCompleted();
            AssertSingleMessageMatches(reference, ClientMessages(feedbackStartSequence));
        }

        private void AssertSingleMessageMatches(string reference, List<JObject> messages)
        {
            messages.Count.ShouldBe(1, "messages sent to the client by the block");
            ReferenceComparer.FromBuildOutput().AssertMatches(reference, messages[0], _harness.RunSecrets);
        }

        private List<JObject> ClientMessages(int afterSequence)
        {
            var messages = new List<JObject>();
            foreach (var interaction in _harness.Interactions)
            {
                if (interaction.Sequence > afterSequence && interaction.Kind == BuilderEngineHarness.InteractionKind.Message)
                {
                    messages.Add(_harness.ToJson(interaction.Envelope));
                }
            }

            return messages;
        }

        private void AssertEngineCompleted()
        {
            _harness.EngineFailure.ShouldBeNull("the engine finished with " + _harness.EngineFailure?.GetType().Name);
        }
    }
}
