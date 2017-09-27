### Human Attendance Forward

In some situations can be necessary use human interventions on chatbot. At this cases is possible use the [BLiP Web](https://web.blip.ai/) to receive and reply messages that came from the chatbots' clients.
All messages replied from BLiP Web will be delivered to the clients as if was sent by the chatbot transparently. 
Obviously it's necessary that some huma reply all messages. This human will be identified as a attendant from now.

To forward any received message to the attendant use the [AttendanceExtension](https://github.com/takenet/blip-sdk-csharp/tree/master/src/Take.Blip.Client/Extensions/AttendanceForwarding/IAttendanceExtension.cs) extension.

Make a call to the `ForwardMessageToAttendantAsync` method informing the attendant cell phone. Note that the cell phone must use informed with international code (XX) and local code (DDD).

The following example show a message forward:

```csharp
public class PlainTextMessageReceiver : IMessageReceiver
{
    private readonly IAttendanceExtension _attendance;

    public PlainTextMessageReceiver(IAttendanceExtension attendanceExtension)
    {
        _attendance = attendanceExtension;
    }

    public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
    {
        // (...)
        // Any logic to define if is necessary forward the message
        await _attendance.ForwardMessageToAttendantAsync(message, "5511XXXXXXXXX", cancellationToken);
    }
}
```
**Note:** It's **required** register an additional `MessageReceiver` to the `application.json` file. This receiver is responsible to redirect automatically any message came from attendant to the final user.

Check this on below example:

```json
{
  "messageReceivers": [
    {
      "mediaType": "application/vnd.omni.attendance\\+json",
      "type": "AttendanceReplyMessageReceiver",
      "settings": {
          "attendantIdentity": "5511XXXXXXXXX"
      }
    }
  ]
}
```

**Note:** It's necessary define the **attendantIdentity** key inside of `settings` property of `MessageReceiver` with the defined attendant cell phone.
