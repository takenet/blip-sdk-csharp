### Installation

The BLiP C# SDK enables the construction of scalable chatbots in a simple and agile way. Its code is [open in Github](https://github.com/takenet/blip-sdk-csharp) and uses the **.NET Core**, which supports several platforms, such as **Windows** , **Linux**, and **Mac**.

The required version of the .NET Core SDK is 2.0 or higher, which is available for installation [here](https://dot.net/core).

To check the installed .NET Core version, run the following command in your operating system's command line interpreter (`Powershell`,` cmd`, `bash`,` terminal`, etc.):

```
dotnet --version
```

The result should be `2.0.0` or newer.

#### Using the project template

The SDK provides [`dotnet` templates](https://github.com/dotnet/templating) to accelerate the creation of chatbots. The templates serve to create the basic structure of projects, including packages and files required to use the application. For example, the `dotnet new mvc` command creates a project using the` mvc` template, which is already pre-installed in the .NET Core SDK, and is used to create *ASP.NET Core MVC* applications. If you want to see which templates are installed on your computer, run the `dotnet new --list` command.

To use the BLiP templates you must, first of all, **install them on your computer**. To do this, run the following command:

```
dotnet new -i Take.Blip.Client.Templates::*
```

The installation of BLiP templates needs to be done *only once*, but can be repeated if you want to upgrade to newer versions. From there, you can create projects using the templates.

The currently available templates are:

- `blip-console` - Create the chatbot as a *Console Application*. It is the template that **should be used for most cases**.
- `blip-web` - Create the chatbot as an *ASP.NET Core application* (experimental). To use this template, your chatbot needs to be of type **Webhook**.

The next step is to create the directory for your chatbot and create a new project from the template:

```
mkdir MyBot
cd MyBot
dotnet new blip-console
```

In this way, a `MyBot.csproj` project and all the files necessary for the operation of your application are created. The suggested editors for working with the BLiP SDK are:

- **Visual Studio 2017** (Update 3) - IDE for Windows. Download the Community version (free) [here] (https://www.visualstudio.com/vs/community/).
- **Visual Studio Code** - Open source code editor, available for Windows, Linux and Mac. Download [here](https://code.visualstudio.com/). It is recommended to install the C # extension.
- **Visual Studio for Mac** - IDE for Mac. Download free [here](https://www.visualstudio.com/vs/visual-studio-mac/).

You will need an identifier and an access key to be able to connect to the BLiP. To get them:
- Access the [BLiP Panel](https://portal.blip.ai).
- On the `Chatbots` tab click` Create chatbot`.
- Step choose the `SDK` option and fill in the requested information
- Ready, your chatbot has been created and the identifier and access key will be displayed.

The identifier and the access key must be defined in the `application.json` file of your project.

To compile the project, run the following command in the application directory:

```
dotnet build
```

To run from source:

```
dotnet run
```

And if you want to run from the compiled binaries, run:

```
dotnet ./bin/Release/MyBot.dll
```

#### Using Programmatically

You can choose to not use a BLiP template and programmatically create and configure your chatbot, using the client only to receive and send messages, notifications and commands. In this case, just install the [package](https://www.nuget.org/packages/Take.Blip.Client) from the client, using the following command:

```
dotnet add package Take.Blip.Client
```

> Note: The SDK documentation considers that the developer is using the project template

To construct a client instance, use the `BlipClientBuilder` class, informing the settings of your chatbot in the methods of this class, and finally calling the `Build()` method to receive an instance of `IBlipClient`, which represents the connection with The platform.

```csharp
// Build a client with the identifier and access key
var client = new BlipClientBuilder()
    .UsingAccessKey("mybot", "V01WNEJtVDBvRVRod1Bycm11Umw=")
    .Build();

// Starts the client, registering handlers to receive the envelopes
await client.StartAsync(
    m =>
    {
        Console.WriteLine("Message '{0}' received from '{1}': {2}", m.Id, m.From, m.Content);
        return client.SendMessageAsync("Pong!", message.From, cancellationToken);
    },
    n =>
    {
        Console.WriteLine("Notification '{0}' received from '{1}': {2}", n.Id, n.From, n.Event);
        return TaskUtil.TrueCompletedTask;
    },
    c =>
    {
        Console.WriteLine("Command '{0}' received from '{1}': {2} {3}", c.Id, c.From, c.Method, c.Uri);
        return TaskUtil.TrueCompletedTask;
    },
    cancellationToken);

Console.WriteLine("Client started. Press enter to stop.");
Console.ReadLine();

// Stops the client
await client.StopAsync(cancellationToken);

Console.WriteLine("Client stopped. Press enter to exit.");
Console.ReadLine();
```
