### Configuring

The configuration of your chatbot is made on `application.json` file. Basically this file define all `receivers` and others control properties.

Check a example of how to set your `application.json` file:

```json
{
  "identifier": "xpto",
  "accessKey": "cXkzT1Rp",
  "messageReceivers": [
    {
      "type": "PlainTextMessageReceiver",
      "mediaType": "text/plain"
    }
  ]
}
```

For this case the client was configured to use a chatbot with `xpto` identifier with `cXkzT1Rp` accessKey. Besides that was registered a **MessageReceiver** with name `PlainTextMessageReceiver` and a filter for messages with `text/plain` **media type**.

Through of `application.json` file the developer can realize a tranparent run of the application. All the other tasks are managed by `mhh.exe` BLiP tool, installed with the template package.

All possible properties of the `application.json` file:

| Property | Description                                                                        | Example                 | Default Value |
|-------------|----------------------------------------------------------------------------------|-------------------------|--------------|
| identifier     | Chatbot identifier. Found this property on [BLiP Portal](http://portal.blip.ai). | myapplication           | null |
| domain      | The **lime** domain to connect. Now only `msging.net` is supported.| msging.net              | msging.net |
| hostName    | The host addres to connect with BLiP server.                                  | msging.net              | msging.net |
| accessKey   | The access key for authentication (using token mode), on **base64** format. Found this property on [BLiP Portal](http://portal.blip.ai).         | MTIzNDU2                 |null |
| password    | The password for authentication (using password mode), on **base64** format.                   | MTIzNDU2                 | null |
| sendTimeout | The timeout value to send messages (in milliseconds).                              | 30000                   | 20000 |
| sessionEncryption | Enchryptation mode.                              | None/TLS                   | TLS |
| sessionCompression | Session compression mode.                              | None                   | None |
| startupType | The name of .NET type that must be started on application load. This type must implements the `IStartable` interface. If the type is located on a different **assembly** of `application.json` file please provide a qualify name with **assembly**.    | Startup (or MyAssemblyName.Startup)     | null |
| serviceProviderType | A type to be used as a service provider for dependency injection. This type must implements the `IServiceProvider` interface. | ServiceProvider | null |
| settings    | General settings of application with key-value format. This value is injected on created types (like **receivers** or **startupType**). To receive the values the constructor of the types must have a instance of `IDictionary<string, object>` type. | { "myApiKey": "abcd1234" }   | null |
| settingsType | The name of .NET type that must be used to deserialize the settings. If the type is located on a different **assembly** of `application.json` file please provide a qualify name with **assembly**.    | ApplicationSettings (or MyAssemblyName.ApplicationSettings)    | null |
| messageReceivers | Array of **message receivers**, used to receive messages. | *See bellow* | null |
| notificationReceivers | Array of **notification receivers**, used to receive notifications. | *See bellow* | null |
| throughput | Envelopes processed limit by second. | 20 | 10 |
| maxConnectionRetries | Reconnection retry limit with server host (1-5). | 3 | 5 |
| routingRule | Routing rule of messages | Instance | Identity |
| disableNotify | Disable automatic notification for messages received and consumed by chatbot | false | false |
| channelCount | Conections count between chatbot and server | 1 | 5 | 
| receiptEvents | Define the events type that the server will foward to the chatbot | [ Accepted, Dispatched, Received, Consumed, Failed ] | [ Received ] |

Each **message receiver** can have the follow properties:

| Property | Description                                                                        | Example                 |
|-------------|----------------------------------------------------------------------------------|-------------------------|
| type        | The name of .NET type to receive messages. This type must implements the `IMessageReceiver` interface. If the type is located on a different **assembly** of `application.json` file please provide a qualify name with **assembly**. | PlainTextMessageReceiver |
| mediaType   | Define a filter of message types that the **receiver** can process. Only messages with this type will be delivered to the receiver. | text/plain |
| content     | Define a regular expression to filter the messages contents that the **receiver** can process. Only messages that satisfy this expression will be delivered to the receiver. | Olá mundo |
| sender     | Define a regular expression to filter the messages originators that the **receiver** can procces. Only messages that satisfy this expression will be delivered to the receiver. | sender@domain.com |
| destination     | Define a regular expression to filter the messages originators that the **receiver** can procces. Only messages that satisfy this expression will be delivered to the receiver. | destination@domain.com |
| settings    | General settings of receiver with key-value format. This value is injected on created types. To receive the values the constructor of the types must have a instance of `IDictionary<string, object>` type. | { "mySetting": "xyzabcd" }   |
| settingsType | The name of .NET type that must be used to deserialize the settings. If the type is located on a different **assembly** of `application.json` file please provide a qualify name with **assembly**.    | PlainTextMessageReceiverSettings     |
| priority | Priority about the other receivers, lower values has more priority. | 0 |
| state | Required originator state to receive message.  | default |
| outState | Define the new originator state after the message is processed. | default |

Each **notification receiver** can have the follow properties:

| Property | Description                                                                        | Example                 |
|-------------|----------------------------------------------------------------------------------|-------------------------|
| type        | The name of .NET type to receive messages. This type must implements the `INotificationReceiver` interface. If the type is located on a different **assembly** of `application.json` file please provide a qualify name with **assembly**. | NotificationReceiver |
| settings    | General settings of receiver with key-value format. This value is injected on created types. To receive the values the constructor of the types must have a instance of `IDictionary<string, object>` type. | { "mySetting": "xyzabcd" }   |
| eventType   | Define a filter of event types that the **receiver** will process. Only notifications for the specified event will be delivered to the receiver. | received |
| settingsType | The name of .NET type that must be used to deserialize the settings. If the type is located on a different **assembly** of `application.json` file please provide a qualify name with **assembly**.    | NotificationReceiverSettings     |
| sender     | Define a regular expression to filter the notifications originators that the **receiver** can procces. Only notifications that satisfy this expression will be delivered to the receiver. | sender@domain.com |
| destination     | Define a regular expression to filter the notifications originators that the **receiver** can procces. Only notifications that satisfy this expression will be delivered to the receiver. | destination@domain.com |
| state | Required originator state to receive notification.  | default |
| outState | Define the new originator state after the notification is processed. | default |
