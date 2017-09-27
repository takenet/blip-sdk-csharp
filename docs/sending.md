### Send

In order to send messages and notifications use an instance of `ISender`, wich is automaticaly injected on constructors of registered `receivers` defined on project and on `Startup` class.

The sample bellow show how to reply a received message:

```csharp
public class PlainTextMessageReceiver : IMessageReceiver
{
    private readonly ISender _sender;

    public PlainTextMessageReceiver(IMessagingHubSender sender)
    {
        _sender = sender;
    }

    public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
    {
        // Write the received message to the console
        Console.WriteLine(message.Content.ToString());
        // Responds to the received message
        _sender.SendMessageAsync("Hi. I just received your message!", message.From, cancellationToken);
    }
}
```

The process of send message is asynchronous and the status of sent messages is delivered to application by **notifications**.

`ISender` interface also enable send **commands** to the server, as the follow sample:

```csharp
var command = new Command {
    Method = CommandMethod.Get,
    Uri = new LimeUri("/account")
};

var response = await _sender.ProcessCommandAsync(command, cancellationToken);
```
For this case, the command response is received on a synchronous way.
