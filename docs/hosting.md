### Hosting

When chatbot is created via SDK, the hosting is the responsibility of the developer. The environment that the application is hosted needs access to the internet in order for the connection to the server to be established.

A *TCP* connection is established on port 443 of the BLiP server. This connection will serve as the transport layer of the [Lime protocol](http://limeprotocol.org/), which is the protocol used for communication.

#### Deployment

With .NET Core, there are two options for deploying your application binaries:

- Framework-dependend: In this mode, it is required that the .NET Core SDK (and its dependencies) be installed on the destination server. The generated binaries are portable.
- Self-contained: In this mode, the native operating system binaries are generated and all dependencies, including the runtime, are included.

For more information, see the [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/deploying/) documentation.

#### Hosting in Windows

Chatbots created through the `blip-console` template can be installed as Windows services if it is running on this operating system. This allows it to continue running on a server without the need for a machine-connected user session.

To install the service on a *framework-dependent*  project, simply run the following command:

```
dotnet MyBot.dll --install --service-name MyBotServiceName --service-description "My BLiP chatbot"
```
NOTE: In Deployment *framework-dependent*, projects of type *Console application* the compiled binary have `.dll` extension.

If you are using the * self-contained * deployment, the command is:

```
MyBot.exe --install --service-name MyBotServiceName --service-description "My BLiP chatbot"
```

Remember that all the contents of your project's compilation output must be present (eg: `Release` folder of the build).

The service created can be started through the Windows `services.msc` utility or through the` sc` command, as below:

```
sc start NomeDoServico
```

To remove the service, use the following command:

```
dotnet MeuBot.dll --uninstall --service-name MyBotServiceName
```

#### Hosting on Linux

Coming soon.

#### Hosting on Docker

Coming soon.

#### Hosting as a web application

Coming soon.
