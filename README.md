
The **BLiP C# SDK** is a set of Nuget packages that allow the creation of [BLiP](https://blip.ai/) chatbots.

<a href="https://www.nuget.org/packages/Take.Blip.Client/" rel="Take.Blip.Client">![NuGet](https://img.shields.io/nuget/v/Take.Blip.Client.svg)</a> 

## Requirements

- .NET Core 3.0 or above (download the SDK [here](https://dotnet.microsoft.com/download))

## Getting Started

The easiest way to get started is using one of our `dotnet` templates. To install the templates, run the execute command in the shell:

```
dotnet new -i "Take.Blip.Client.Templates::*"
```

After installing the templates, just create a directory for your chatbot and create a new project using a template:

```
mkdir MyBot
cd MyBot
dotnet new blip-console
```

There are available the following templates:
- `blip-console` - Run as a console application
- `blip-web` - Run as a ASP.NET Core application (experimental)

For more details about the BLiP SDK, please refer to our SDK documentation [here](https://docs.blip.ai/#sdk).

## License

[Apache 2.0 License](https://github.com/takenet/blip-sdk-csharp/blob/master/LICENSE)
