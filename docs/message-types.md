### Message Types

**BLiP Messaging Hub** define message types (**cannonical types**) that are automatically parsed to the best available representation on each channel. For more details check the *Concepts > Messages* section of documentation.

### Text Message (PlainText)

Simple text messages are supported for any channel. However, each channel can have restrictions as for sample the message length.

*Example:*

The sample follow show how to reply a received message with a simple text message.
```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    var document = new PlainText {Text = "... Inspiration, and a cup of coffe! It's enough!"};
    await _sender.SendMessageAsync(document, message.From, cancellationToken);
}
```
*Limitations:*

- Facebook Messenger: Max size of 320 characters. If your chatbot send messages with more than 320 characters, on this channel, your message will not be delivered.

### Links to media files and web pages (MediaLink e WebLink)

To send media links, the message sent must have a MediaLink document as follow:
```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    var imageUri = new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/4/45/A_small_cup_of_coffee.JPG/200px-A_small_cup_of_coffee.JPG", UriKind.Absolute);

    var document = new MediaLink
    {
        Text = "Coffe, what else ?",
        Size = 6679,
        Type = MediaType.Parse("image/jpeg"),
        PreviewUri = imageUri,
        Uri = imageUri
    };
    await _sender.SendMessageAsync(document, message.From, cancellationToken);
}
```
To send a web page link use the WebLink type:
```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    var url = new Uri("https://pt.wikipedia.org/wiki/Caf%C3%A9");
    var previewUri =
        new Uri(
            "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Roasted_coffee_beans.jpg/200px-Roasted_coffee_beans.jpg");

    var document = new WebLink
    {
        Text = "Coffe, the god's drink!",
        PreviewUri = previewUri,
        Uri = url
    };

    await _sender.SendMessageAsync(document, message.From, cancellationToken);
}
```
### Sending an options list (Select)

Send an options list to give your client the choice between multiple answers using Select type:
```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    var document = new Select
    {
        Text = "Choice an option:",
        Options = new []
        {
            new SelectOption
            {
                Order = 1,
                Text = "An inspire text!",
                Value = new PlainText { Text = "1" }
            },
            new SelectOption
            {
                Order = 2,
                Text = "A motivational image!",
                Value = new PlainText { Text = "2" }
            },
            new SelectOption
            {
                Order = 3,
                Text = "An interesting link!",
                Value = new PlainText { Text = "3" }
            }
        }
    };

    await _sender.SendMessageAsync(document, message.From, cancellationToken);
}
```
**Note:**
- `Value` field is optional, if informed your value will be sent to the chatbot when the user choice the option.
- If `Value` field is not provided, will must provide one of the fields: `Order` or `Text`. The `Order` field will be used only if `Value` and `Text` is not provided.

**Limitations:**
- Facebook Messenger: Limite of 3 options, in other case your message will not be delivered. If is nedded to send more than 3 options is necessary send multiple messages.
- Tangram SMS: The `Value` field will be ignored. Only the `Order` field will be sent if the option be selected.

### Geolocation (Location)

A chatbot can send and receive a location entity. For this cases use Location type:
```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    var document = new Location
    {
        Latitude = -22.121944,
        Longitude = -45.128889,
        Altitude = 1143
    };

    await _sender.SendMessageAsync(document, message.From, cancellationToken);
}
```

### Processing Payments (Invoice, InvoiceStatus and PaymentReceipt)

In order to realize a payment on your chatbot is necessary use the payment channel. For now, only the PagSeguro channel is supported and to request a payment the chatbot must send a message of type Invoice to the payment channel informing the user address using the format bellow:
```csharp
var toPagseguro = $"{Uri.EscapeDataString(message.From.ToIdentity().ToString())}@pagseguro.gw.msging.net"; // Ex: 5531988887777%400mn.io@pagseguro.gw.msging.net
```
Check a complete example of payment request:
```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    var document = new Invoice
    {
        Currency = "BLR",
        DueTo = DateTime.Now.AddDays(1),
        Items =
            new[]
            {
                new InvoiceItem
                {
                    Currency = "BRL",
                    Unit = 1,
                    Description = "Some product",
                    Quantity = 1,
                    Total = 1
                }
            },
        Total = 1
    };

    var toPagseguro = $"{Uri.EscapeDataString(message.From.ToIdentity().ToString())}@pagseguro.gw.msging.net";

    await _sender.SendMessageAsync(document, toPagseguro, cancellationToken);
}
```
**Important:**
- Before to send a payment request you must enable the PagSeguro payment channel for your chatbot on **BLiP Messaging Hub Portal**.

When PagSeguro payment channel receive a payment request it send a link to the client. If the client finalize or cancel the payment a message of InvoiceStatus type will be delivered to your chatbot. To receive this message your chatbot must have a *Receiver* for `application/vnd.lime.invoice-status+json` MediaType registered on `application.json`, as follow:

```js
"messageReceivers": [
{
    {
        "type": "InvoiceStatusReceiver",
        "mediaType": "application/vnd.lime.invoice-status\\+json"
    }
}
```
The InvoiceStatusReceiver must be defined as follow:
```csharp
public class InvoiceStatusReceiver : IMessageReceiver
{
    private readonly IMessagingHubSender _sender;

    public InvoiceStatusReceiver(IMessagingHubSender sender)
    {
        _sender = sender;
    }

    public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
    {
        var invoiceStatus = message.Content as InvoiceStatus;
        switch (invoiceStatus?.Status)
        {
            case InvoiceStatusStatus.Cancelled:
                await _sender.SendMessageAsync("Ok, you don't need pay anything.", message.From, cancellationToken);
                break;
            case InvoiceStatusStatus.Completed:
                await _sender.SendMessageAsync("Thank you for your payment, this is only a test", message.From, cancellationToken);
                var paymentReceipt = new PaymentReceipt
                {
                    Currency = "BLR",
                    Items =
                        new[]
                        {
                            new InvoiceItem
                            {
                                Currency = "BRL",
                                Unit = 1,
                                Description = "Some product",
                                Quantity = 1,
                                Total = 1
                            }
                        },
                    Total = 1
                };
                await _sender.SendMessageAsync(paymentReceipt, message.From, cancellationToken);
                break;
            case InvoiceStatusStatus.Refunded:
                await _sender.SendMessageAsync("Ok, your payment was refunded by PagSeguro!", message.From, cancellationToken);
                break;
        }
    }
}
```

As showed by the example above your chatbot must be prepared to process 3 differents states of a payment request (`InvoiceStatusStatus.Cancelled`, `InvoiceStatusStatus.Completed`, `InvoiceStatusStatus.Refunded`) and must reply a PaymentReceipt to the client when the payment was executed if success.

### Composed Messages (DocumentCollection e DocumentContainer)

Composed messages can be sent using the document types DocumentCollection or DocumentContainer, as the following example:

```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    var imageUrl = new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/4/45/A_small_cup_of_coffee.JPG/200px-A_small_cup_of_coffee.JPG");
    var url = new Uri("https://pt.wikipedia.org/wiki/Caf%C3%A9");
    var previewUrl = new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Roasted_coffee_beans.jpg/200px-Roasted_coffee_beans.jpg");

    var document = new DocumentCollection
    {
        ItemType = DocumentContainer.MediaType,
        Items = new[]
        {
            new DocumentContainer
            {
                Value = new PlainText {Text = "An inspire text!"}
            },
            new DocumentContainer
            {
                Value = new MediaLink
                {
                    Text = "Coffe, what else ?",
                    Size = 6679,
                    Type = MediaType.Parse("image/jpeg"),
                    PreviewUri = imageUrl,
                    Uri = imageUrl
                }
            },
            new DocumentContainer
            {
                Value = new WebLink
                {
                    Text = "Coffe, the God's drink!",
                    PreviewUri = previewUrl,
                    Uri = url
                }
            }
        }
    };

    await _sender.SendMessageAsync(document, message.From, cancellationToken);
}
```