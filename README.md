
The **BLiP C# SDK** is a set of Nuget packages that allow the creation of [BLiP](https://blip.ai/) chatbots.

<a href="https://www.nuget.org/packages/Take.Blip.Client/" rel="Take.Blip.Client">![NuGet](https://img.shields.io/nuget/v/Take.Blip.Client.svg)</a>

## Requirements

- .NET Core 2.0 (download the SDK [here](https://dot.net/core))

## Getting Started

The easiest way to get started is using one of our `dotnet` templates. To install the templates, run the execute command in the shell:

```
dotnet new -i Take.Blip.Client.ConsoleTemplate::*
```

After installing the templates, just create a directory for your chatbot and create a new project using a template:

```
mkdir MyBot
cd MyBot
dotnet new blip-console
```

For more details about the BLiP SDK, please refer to our SDK documentation [here](https://portal.blip.ai/#/docs/home).

## License

[Apache 2.0 License](https://github.com/takenet/blip-sdk-csharp/blob/master/LICENSE)
