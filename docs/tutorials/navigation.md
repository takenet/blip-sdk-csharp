### Tutorial: Navigation

> *This article was automatically translated and may have some errors. If you find any problem, please help us improving it following the link in the end of this page.*

In this tutorial, we will demonstrate a way to build a chatbot that automatically responds to text commands sent by users.

The first step is to create a new project using the blip-console template in the command line:

```
dotnet new blip-console
```
  
In this way, the `application.json` file is added to the project, among other dependencies, where the *receivers* of messages and notifications are registered. **receivers** are the entities responsible for processing messages and notifications received by performing specific actions (invoking APIs, saving information in the database, etc.) and, if necessary, sending a response to the user.

An important and very useful detail is that you can register * receivers * by defining **filters** of messages and notifications that it must process. Filters can combine various properties, such as the originator and message content, for example. In addition, they are **regular expressions** that allow for greater flexibility in their definition.

Below is an example of the `application.json` file created in a new project:

```json
{
  "identifier": "",
  "accessKey": "",
  "messageReceivers": [
    {
      "type": "PlainTextMessageReceiver",
      "mediaType": "text/plain"
    }
  ],
  "settings": {
    "setting1": "value1"
  },
  "startupType": "Startup",
  "schemaVersion": 2
}
```
> To obtain an `identifier` and` accessKey`, go to the portal https://portal.blip.ai and register your chatbot using the Chat Bot SDK option

In this case, there is only a **message** registered message, with a `text / plain` type content filter being processed by the` PlainTextMessageReceiver` class that must exist in the project.

Imagine that our chatbot should respond to the command with the `help` text with a static message of assistance to the user. In this way, we need:
- Register a new message receiver
- Include a filter of type *text* and content *help*
- Return the help message to the originator

For ease of use, the SDK includes some *receivers* for common actions, such as static messages, and it is not necessary in this first use case to implement the *receiver* to send the response message. To do this, simply use the `response` property and include the response message to the client. In this case, the `messageReceivers` session would look like this:

```json
  "messageReceivers": [
    {
      "mediaType": "text/plain",
      "content": "ajuda",
      "response": {
        "mediaType": "text/plain",
        "plainContent": "Olá, seja bem-vindo ao serviço de ajuda do Messaging Hub."
      }
    }
  ]
```
In this way, if the client sends the word `help`, it will receive a message of type `text/plain` with content `Hello, welcome to the help service of the Messaging Hub.`. If we want to include other words for the activation of the command, simply change the `content` property and include other words in the regular filter expression, as follows:

```json
  "messageReceivers": [
    {
      "mediaType": "text/plain",
      "content": "^(inicio|iniciar|começar|ajuda)$",
      "response": {
        "mediaType": "text/plain",
        "plainContent": "Olá, seja bem-vindo ao serviço de ajuda do Messaging Hub."
      }
    }
  ]
```
We can return, instead of a plain text, a complex message type such as a ** Select **, which shows a menu of options to the user. To do this, simply use the `jsonContent` property instead of` plainContent`, as below:

```json
  "messageReceivers": [
    {
      "mediaType": "text/plain",
      "content": "^(inicio|iniciar|começar|ajuda)$",
      "response": {
        "mediaType": "application/vnd.lime.select+json",
        "jsonContent": {
          "text": "Olá, seja bem-vindo ao serviço de ajuda do Messaging Hub. Escolha o que você deseja receber:",
          "options": [
            {
              "order": 1,
              "text": "Um TEXTO",
              "type": "text/plain",
              "value": "texto"
            },
            {
              "order": 2,
              "text": "Uma IMAGEM",
              "type": "text/plain",
              "value": "imagem"
            },
            {
              "order": 3,
              "text": "A DATA atual",
              "type": "text/plain",
              "value": "data"
            }
          ]
        }
      }
    }
  ]
```
For each of the `Select` options, we must include a *receiver* for the word set to` value`, which is the value expected as the customer's response. But it is also important to support sending the number and the text of the option, since in unstructured channels (such as SMS) there is no guarantee that the customer will respond with the expected option. A receiver for the first option (`text`) would be:

```json
    {
      "mediaType": "text/plain",
      "content": "^(texto|um texto|1)$",
      "response": {
        "mediaType": "text/plain",
        "plainContent": "Este é um texto simples, sem nada de especial."
      }
    }
```    
In this case, we return a simple message but supporting several different commands to activate the receiver. For the second option, we have:

```json
    {
      "mediaType": "text/plain",
      "content": "^(imagem|uma imagem|2)$",
      "response": {
        "mediaType": "application/vnd.lime.media-link+json",
        "jsonContent": {
          "type": "image/jpeg",
          "uri": "http://static.boredpanda.com/blog/wp-content/uploads/2015/09/Instagrams-most-famous-cat-Nala165604f5fc88e5f.jpg",
          "text": "Meaww!"
        }
      }
    }
```    

Here we return a complex `MediaLink` type with an image. The third option (`data`) includes dynamic content and for this reason, we can not use the` response` property. Therefore, we must create a class to process the text and respond to the user, as follows:

```csharp
    public class DateMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private readonly CultureInfo _cultureInfo;
        private readonly string _messageTemplate;

        public DateMessageReceiver(ISender sender, IDictionary<string, object> settings)
        {
            _sender = sender;
            if (settings.ContainsKey("culture"))
            {            
                _cultureInfo = new CultureInfo((string)settings["culture"]);
            }
            else
            {
                _cultureInfo = CultureInfo.InvariantCulture;
            }

            _messageTemplate = (string)settings["message"];
        }

        public Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = new CancellationToken())
        {
            return _sender.SendMessageAsync(string.Format(_messageTemplate, DateTime.Now.ToString("g", _cultureInfo)), envelope.From, cancellationToken);
        }
    }
```

Our class receives by the constructor its **configurations** which include the response and culture text template, which are defined in the * receiver * record in the `application.json` file. It is always a good idea to use the `settings` property to set static values, which allows you to modify the behavior of your chatbot without the need to recompile the code. The record would look like this:

```json
    {
      "mediaType": "text/plain",
      "content": "^(data|a data atual|3)$",
      "type": "DateMessageReceiver",
      "settings": {
        "culture": "pt-BR",
        "message": "The current date is {0}."
      }
    }
```
Finally, imagine that your chatbot should return a static error message in case the client sends some unknown command. For this, it is necessary to register a * receiver * without filters but with **priority** lower than the other *receivers* existing. By default, *receivers* are registered with the highest priority (**zero**) and simply include a *receiver* with a lower priority so that it receives messages not handled by others to respond to the user. It would look like this:

```json
    {
      "priority": "100",
      "response": {
        "mediaType": "text/plain",
        "plainContent": "Ops, I didn't understand what you mean. Send the text HELP if you need."
      }
    }
```    
The complete code for this tuturial can be found at [Github](https://github.com/takenet/blip-sdk-csharp/tree/master/src/Samples/Navigation).
