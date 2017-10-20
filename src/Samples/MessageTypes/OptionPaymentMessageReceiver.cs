using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionPaymentMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionPaymentMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Document document;
            // O método de pagamento deve ser informado no portal do Messaging Hub
            if (message.Content.ToString().Equals("pm1"))
            {
                document = getPaymentInvoice();
                var toPagseguro = $"{Uri.EscapeDataString(message.From.ToIdentity().ToString())}@pagseguro.gw.msging.net";
                await _sender.SendMessageAsync(document, toPagseguro, cancellationToken);
            }
            else
                document = await getPaymentReceipt(message, cancellationToken);


            // O fluxo continua o Option6Part2MessageReceiver
        }

        public Invoice getPaymentInvoice()
        {
            var invoice = new Invoice
            {
                Currency = "BLR",
                DueTo = DateTime.Now.AddDays(1),
                Items = new InvoiceItem[]
                    {
                        new InvoiceItem
                        {
                            Currency = "BRL",
                            Unit = 10,
                            Description = "Subscription for product: Hit",
                            Quantity = 1,
                            Total = 10
                        }
                    },
                Total = 1
            };
            return invoice;
        }

        public async Task<InvoiceStatus> getPaymentReceipt(Message message, CancellationToken cancellationToken)
        {
            var invoiceStatus = message.Content as InvoiceStatus;
            switch (invoiceStatus?.Status)
            {
                case InvoiceStatusStatus.Cancelled:
                    await _sender.SendMessageAsync("Tudo bem, n�o precisa pagar nada.", message.From, cancellationToken);
                    break;
                case InvoiceStatusStatus.Completed:
                    await _sender.SendMessageAsync("Obrigado pelo seu pagamento, mas como isso � apenas um teste, voc� pode pedir o ressarcimento do valor pago ao PagSeguro. Em todo caso, segue o seu recibo:", message.From, cancellationToken);
                    var paymentReceipt = new PaymentReceipt
                    {
                        
                        Currency = "BLR",
                        Total = 10,
                        Items =
                            new[]
                            {
                                new InvoiceItem
                                {
                                    Currency = "BRL",
                                    Unit = 10,
                                    Description = "Item 1",
                                    Quantity = 1,
                                    Total = 10
                                }
                            },
                    };
                    await _sender.SendMessageAsync(paymentReceipt, message.From, cancellationToken);
                    break;
                case InvoiceStatusStatus.Refunded:
                    await _sender.SendMessageAsync("Pronto. O valor que voc� me pagou j� foi ressarcido pelo PagSeguro!", message.From, cancellationToken);
                    break;
            }
            return invoiceStatus;
        }
    }
}