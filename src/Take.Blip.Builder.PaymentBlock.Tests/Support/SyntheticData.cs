using System;
using System.Security.Cryptography;

namespace Take.Blip.Builder.PaymentBlock.Tests.Support
{
    /// <summary>
    /// Dados sintéticos fixos da execução do bloco no motor. Nenhum valor real: hosts em <c>*.example.test</c>,
    /// identidades sem telefone e chave PIX CNPJ de 14 zeros. Segredo do bucket e token da auth-api são gerados a cada
    /// execução por <see cref="NewRunSecret"/>.
    /// </summary>
    public static class SyntheticData
    {
        /// <summary>Identidade do cliente que conversa com o bot.</summary>
        public const string USER_IDENTITY = "cliente-de-teste@wa.gw.msging.net";

        /// <summary>Identificador do bot (aplicação) dono do fluxo.</summary>
        public const string APPLICATION_IDENTIFIER = "bot-de-teste";

        /// <summary>Domínio da identidade do bot.</summary>
        public const string APPLICATION_DOMAIN = "msging.net";

        /// <summary>Nome do contato devolvido pela extensão de contatos simulada.</summary>
        public const string CONTACT_NAME = "Cliente de Teste";

        /// <summary>Texto da mensagem do cliente que leva o fluxo ao bloco de Pagamentos.</summary>
        public const string CLIENT_MESSAGE_TEXT = "Quero pagar o meu pedido";

        /// <summary>Identificador do fluxo montado pelo <see cref="FlowAssembler"/>.</summary>
        public const string FLOW_ID = "fluxo-de-teste-checkout-v1";

        /// <summary>Host da checkout-api, o mesmo das ações do bloco na fixture.</summary>
        public const string CHECKOUT_HOST = "checkout.hmg.example.test";

        /// <summary>Host da checkout-auth-api, o mesmo das ações do bloco na fixture.</summary>
        public const string AUTH_HOST = "auth.hmg.example.test";

        /// <summary>Id fixo da compra criada pela checkout-api simulada.</summary>
        public const string PURCHASE_ID = "compra-de-teste-0001";

        /// <summary>Link de pagamento determinístico devolvido pela checkout-api simulada.</summary>
        public const string PAYMENT_LINK = "https://checkout.hmg.example.test/pagamento?id=compra-de-teste-0001";

        /// <summary>Código PIX copia e cola devolvido pela checkout-api simulada.</summary>
        public const string PIX_COPY_CODE = "pix-copia-e-cola-de-teste";

        /// <summary>Chave PIX CNPJ do merchant: 14 dígitos sem formatação, todos zero.</summary>
        public const string PIX_KEY_CNPJ = "00000000000000";

        /// <summary>Nome do merchant devolvido junto com o PIX.</summary>
        public const string PIX_MERCHANT_NAME = "Loja de Teste";

        /// <summary>Id do cartão salvo devolvido pela checkout-api simulada.</summary>
        public const string SAVED_CARD_ID = "cartao-de-teste-0001";

        /// <summary>Final do cartão salvo devolvido pela checkout-api simulada.</summary>
        public const string SAVED_CARD_LAST_FOUR = "0000";

        private const int RUN_SECRET_BYTES = 32;

        /// <summary>
        /// Gera um valor aleatório (base64 url-safe, 43 caracteres) para segredo ou token desta execução.
        /// </summary>
        public static string NewRunSecret()
        {
            var bytes = RandomNumberGenerator.GetBytes(RUN_SECRET_BYTES);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
