using System;
using System.Security.Cryptography;

namespace Take.Blip.Builder.PaymentBlock.Tests.Support
{
    /// <summary>
    /// Fixed synthetic data for the block run on the engine. No real value: hosts in <c>*.example.test</c>,
    /// identities without a phone number and a CNPJ PIX key of 14 zeros. The bucket secret and the auth-api token are
    /// generated on every run by <see cref="NewRunSecret"/>.
    /// </summary>
    public static class SyntheticData
    {
        /// <summary>Identity of the customer talking to the bot.</summary>
        public const string USER_IDENTITY = "cliente-de-teste@wa.gw.msging.net";

        /// <summary>Identifier of the bot (application) that owns the flow.</summary>
        public const string APPLICATION_IDENTIFIER = "bot-de-teste";

        /// <summary>Domain of the bot identity.</summary>
        public const string APPLICATION_DOMAIN = "msging.net";

        /// <summary>Contact name returned by the simulated contact extension.</summary>
        public const string CONTACT_NAME = "Cliente de Teste";

        /// <summary>Text of the customer message that takes the flow to the Payments block.</summary>
        public const string CLIENT_MESSAGE_TEXT = "Quero pagar o meu pedido";

        /// <summary>Identifier of the flow assembled by <see cref="FlowAssembler"/>.</summary>
        public const string FLOW_ID = "fluxo-de-teste-checkout-v1";

        /// <summary>checkout-api host, the same one used by the block actions in the fixture.</summary>
        public const string CHECKOUT_HOST = "checkout.hmg.example.test";

        /// <summary>checkout-auth-api host, the same one used by the block actions in the fixture.</summary>
        public const string AUTH_HOST = "auth.hmg.example.test";

        /// <summary>Fixed id of the purchase created by the simulated checkout-api.</summary>
        public const string PURCHASE_ID = "compra-de-teste-0001";

        /// <summary>Deterministic payment link returned by the simulated checkout-api.</summary>
        public const string PAYMENT_LINK = "https://checkout.hmg.example.test/pagamento?id=compra-de-teste-0001";

        /// <summary>PIX copy-and-paste code returned by the simulated checkout-api.</summary>
        public const string PIX_COPY_CODE = "pix-copia-e-cola-de-teste";

        /// <summary>Merchant CNPJ PIX key: 14 unformatted digits, all zero.</summary>
        public const string PIX_KEY_CNPJ = "00000000000000";

        /// <summary>Merchant name returned along with the PIX.</summary>
        public const string PIX_MERCHANT_NAME = "Loja de Teste";

        /// <summary>Id of the saved card returned by the simulated checkout-api.</summary>
        public const string SAVED_CARD_ID = "cartao-de-teste-0001";

        /// <summary>Last digits of the saved card returned by the simulated checkout-api.</summary>
        public const string SAVED_CARD_LAST_FOUR = "0000";

        private const int RUN_SECRET_BYTES = 32;

        /// <summary>
        /// Generates a random value (url-safe base64, 43 characters) for a secret or token of this run.
        /// </summary>
        public static string NewRunSecret()
        {
            var bytes = RandomNumberGenerator.GetBytes(RUN_SECRET_BYTES);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
