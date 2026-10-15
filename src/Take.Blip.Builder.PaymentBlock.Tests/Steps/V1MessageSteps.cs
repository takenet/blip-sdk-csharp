using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Reqnroll;
using Shouldly;
using Take.Blip.Builder.PaymentBlock.Tests.Comparison;
using Take.Blip.Builder.PaymentBlock.Tests.Support;

namespace Take.Blip.Builder.PaymentBlock.Tests.Steps
{
    /// <summary>
    /// Steps of <c>mensagens_do_bloco_v1.feature</c>: runs the v1 block with a row of <c>configuracoes-v1.json</c> and compares
    /// the message sent to the customer with the approved reference in <c>Referencias/</c>, through the <see cref="ReferenceComparer"/>.
    /// Sending the customer message is the shared step of <see cref="BlockExecutionSteps"/>.
    /// </summary>
    [Binding]
    public sealed class V1MessageSteps
    {
        private const string BUCKET_ID = "checkout-secret";

        private readonly BuilderEngineHarness _harness;
        private int _feedbackStartSequence;

        /// <summary>Receives the scenario harness through Reqnroll context injection.</summary>
        public V1MessageSteps(BuilderEngineHarness harness)
        {
            _harness = harness ?? throw new ArgumentNullException(nameof(harness));
        }

        /// <summary>Loads the flow with the configuration row without feedback and stores this run's secret in the bucket.</summary>
        [Given("o fluxo de referência {string} com a configuração {string}")]
        public void GivenReferenceFlowWithConfiguration(string referenceFlow, string configuration)
        {
            _harness.UseReferenceFlow(referenceFlow, configuration, false);
            _harness.StoreBucketSecret(BUCKET_ID);
        }

        /// <summary>Loads the flow with the configuration row with feedback and stores this run's secret in the bucket.</summary>
        [Given("o fluxo de referência {string} com a configuração {string} e feedback ligado")]
        public void GivenReferenceFlowWithConfigurationAndFeedback(string referenceFlow, string configuration)
        {
            _harness.UseReferenceFlow(referenceFlow, configuration, true);
            _harness.StoreBucketSecret(BUCKET_ID);
        }

        /// <summary>Puts the simulated checkout-api in normal mode.</summary>
        [Given("a checkout-api simulada respondendo normalmente")]
        public void GivenCheckoutApiRespondingNormally()
        {
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Normal;
        }

        /// <summary>Makes the payment query in the simulated checkout-api report the payment method and status.</summary>
        [Given("a checkout-api simulada informando o pagamento por {string} com status {string}")]
        public void GivenCheckoutApiReportingPayment(string paymentMethod, string status)
        {
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Normal;
            _harness.HttpClient.ReportPayment(paymentMethod, status);
        }

        /// <summary>
        /// Takes the customer to the block and delivers to the engine, without real waiting, the input expiration it scheduled: it is
        /// the end of the feedback time, which triggers the payment query.
        /// </summary>
        [When("o tempo de feedback termina")]
        public async Task WhenFeedbackTimeEnds()
        {
            await _harness.SendClientMessageAsync();
            AssertEngineCompleted();
            _feedbackStartSequence = _harness.Interactions.Count;
            await _harness.DeliverScheduledInputExpirationAsync();
            AssertEngineCompleted();
        }

        /// <summary>Checks that the block sent a single message to the customer and compares it with the given reference.</summary>
        [Then("a mensagem enviada ao cliente é igual à referência {string}, exceto campos voláteis")]
        public void ThenMessageSentMatchesReference(string reference)
        {
            AssertEngineCompleted();
            AssertSingleMessageMatches(reference, ClientMessages(0));
        }

        /// <summary>
        /// Checks that the end of the feedback time produced a single message to the customer and compares it with the given reference.
        /// </summary>
        [Then("a mensagem de status enviada é igual à referência {string}, exceto campos voláteis")]
        public void ThenStatusMessageSentMatchesReference(string reference)
        {
            AssertEngineCompleted();
            AssertSingleMessageMatches(reference, ClientMessages(_feedbackStartSequence));
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
