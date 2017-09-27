### Receiving

The receipt of messages and notifications is done using the interaces `IMessageReceiver` and `INotificationReceiver` respectively.

A `IMessageReceiver` can be defined as follow:

```csharp
public class PlainTextMessageReceiver : IMessageReceiver
{
    public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
    {
        // Write the received message to the console
        Console.WriteLine(message.Content.ToString());
    }
}
```

Some important notes:

- Before the `ReceiveAsync` method be executed, a notification of `Event.Received` type is automatically sent to originator of message.
- After `ReceiveAsync` method be executed, if no one exception occur, a notification of type `Event.Consumed` is automatically sent to originator of message.
- If some exception occur on `ReceiveAsync` method, a notificação of type `Event.Failed` is automatically sent to originator of message.

A `INotificationReceiver` can be defined as follow:

```csharp
public class ConsumedNotificationReceiver : INotificationReceiver
{
    public async Task ReceiveAsync(Notification notification, CancellationToken cancellationToken)
    {
        // Write the received notification to the console
        Console.WriteLine(notification.ToString());
    }
}
```

The notifcations are *fire-and-forget* and if occur some exception on `ReceiveAsync`, this fail will be ignored.

Note: Remember to register all implementations of `INotificationReceiver` and `IMessageReceiver` on `application.json` file. For more informations check the **Configuring** section.
