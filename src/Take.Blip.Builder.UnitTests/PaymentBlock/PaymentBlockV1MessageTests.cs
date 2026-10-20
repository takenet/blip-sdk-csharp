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
    /// the message sent to the customer with the approved reference in <c>PaymentBlock/Referencias/</c>, through the
    /// <see cref="ReferenceComparer"/>. The references are used by the payment-application to prove it generates the same messages
    /// when the block logic leaves the front end.
    /// </summary>
    [Trait(CATEGORY, REGRESSION)]
    [Trait(CATEGORY, PAYMENT_BLOCK)]
    [Trait(CATEGORY, V1_REFERENCE)]
    public sealed class PaymentBlockV1MessageTests : IDisposable
    {
        private const string CATEGORY = "Category";
        private const string REGRESSION = "regressao";
        private const string PAYMENT_BLOCK = "bloco-pagamento";
        private const string V1_REFERENCE = "referencia-v1";
        private const string REFERENCE_FLOW = "checkout-v1";
        private const string FEEDBACK_CONFIGURATION = "checkout-link-e-pix";
        private const string BUCKET_ID = "checkout-secret";

        private readonly BuilderEngineHarness _harness = new BuilderEngineHarness();

        public void Dispose()
        {
            _harness.Dispose();
        }

        /// <summary>Mensagem do pedido para cada configuração do bloco.</summary>
        [Theory]
        [InlineData("checkout-link", "v1-checkout-link")]
        [InlineData("checkout-pix", "v1-checkout-pix")]
        [InlineData("checkout-link-e-pix", "v1-checkout-link-e-pix")]
        [InlineData("checkout-link-com-cartao-salvo", "v1-checkout-cta-cartao")]
        [InlineData("proprio-pix-cpf", "v1-proprio-pix-cpf")]
        [InlineData("proprio-pix-aleatoria", "v1-proprio-pix-aleatoria")]
        [InlineData("proprio-link", "v1-proprio-link")]
        [InlineData("proprio-boleto", "v1-proprio-boleto")]
        [InlineData("produto-fisico-dois-itens", "v1-fisico-dois-itens")]
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
        /// Mensagem de status para cada resultado do pagamento. The customer reaches the block and the engine receives, without real
        /// waiting, the input expiration it scheduled: the end of the feedback time, which triggers the payment query.
        /// </summary>
        [Theory]
        [InlineData("PIX", "Succeeded", "v1-status-pix-pago")]
        [InlineData("CreditCard", "Succeeded", "v1-status-cartao-pago")]
        [InlineData("CreditCard", "Denied", "v1-status-negado")]
        [InlineData("PIX", "Pending", "v1-status-pendente")]
        [InlineData("PIX", "Created", "v1-status-criado")]
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
