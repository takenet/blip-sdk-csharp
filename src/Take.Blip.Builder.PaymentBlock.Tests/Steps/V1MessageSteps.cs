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
    /// Passos de <c>mensagens_do_bloco_v1.feature</c>: roda o bloco v1 com uma linha de <c>configuracoes-v1.json</c> e compara
    /// a mensagem enviada ao cliente com a referência aprovada de <c>Referencias/</c>, pelo <see cref="ReferenceComparer"/>.
    /// O envio da mensagem do cliente é o passo compartilhado de <see cref="BlockExecutionSteps"/>.
    /// </summary>
    [Binding]
    public sealed class V1MessageSteps
    {
        private const string BUCKET_ID = "checkout-secret";

        private readonly BuilderEngineHarness _harness;
        private int _feedbackStartSequence;

        /// <summary>Recebe o harness do cenário pela injeção de contexto do Reqnroll.</summary>
        public V1MessageSteps(BuilderEngineHarness harness)
        {
            _harness = harness ?? throw new ArgumentNullException(nameof(harness));
        }

        /// <summary>Carrega o fluxo com a linha de configuração sem feedback e guarda no bucket o segredo desta execução.</summary>
        [Given("o fluxo de referência {string} com a configuração {string}")]
        public void GivenReferenceFlowWithConfiguration(string referenceFlow, string configuration)
        {
            _harness.UseReferenceFlow(referenceFlow, configuration, false);
            _harness.StoreBucketSecret(BUCKET_ID);
        }

        /// <summary>Carrega o fluxo com a linha de configuração com feedback e guarda no bucket o segredo desta execução.</summary>
        [Given("o fluxo de referência {string} com a configuração {string} e feedback ligado")]
        public void GivenReferenceFlowWithConfigurationAndFeedback(string referenceFlow, string configuration)
        {
            _harness.UseReferenceFlow(referenceFlow, configuration, true);
            _harness.StoreBucketSecret(BUCKET_ID);
        }

        /// <summary>Deixa a checkout-api simulada no modo normal.</summary>
        [Given("a checkout-api simulada respondendo normalmente")]
        public void GivenCheckoutApiRespondingNormally()
        {
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Normal;
        }

        /// <summary>Faz a consulta do pagamento na checkout-api simulada informar o meio e o status do pagamento.</summary>
        [Given("a checkout-api simulada informando o pagamento por {string} com status {string}")]
        public void GivenCheckoutApiReportingPayment(string paymentMethod, string status)
        {
            _harness.HttpClient.CheckoutApi = SimulatedHttpClient.CheckoutApiMode.Normal;
            _harness.HttpClient.ReportPayment(paymentMethod, status);
        }

        /// <summary>
        /// Leva o cliente ao bloco e entrega ao motor, sem espera real, a expiração de input que ele agendou: é o fim do
        /// tempo de feedback, que dispara a consulta do pagamento.
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

        /// <summary>Confere que o bloco enviou uma única mensagem ao cliente e a compara com a referência informada.</summary>
        [Then("a mensagem enviada ao cliente é igual à referência {string}, exceto campos voláteis")]
        public void ThenMessageSentMatchesReference(string reference)
        {
            AssertEngineCompleted();
            AssertSingleMessageMatches(reference, ClientMessages(0));
        }

        /// <summary>
        /// Confere que o fim do tempo de feedback gerou uma única mensagem ao cliente e a compara com a referência informada.
        /// </summary>
        [Then("a mensagem de status enviada é igual à referência {string}, exceto campos voláteis")]
        public void ThenStatusMessageSentMatchesReference(string reference)
        {
            AssertEngineCompleted();
            AssertSingleMessageMatches(reference, ClientMessages(_feedbackStartSequence));
        }

        private void AssertSingleMessageMatches(string reference, List<JObject> messages)
        {
            messages.Count.ShouldBe(1, "mensagens enviadas ao cliente pelo bloco");
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
            _harness.EngineFailure.ShouldBeNull("o motor terminou com " + _harness.EngineFailure?.GetType().Name);
        }
    }
}
