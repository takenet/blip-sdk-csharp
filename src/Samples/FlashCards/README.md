# bot-flash-cards-blip-sdk-csharp

A little open source project in flash cards game format.

## Requirements

 * NET Core 2.0 or above. Download the SDK [here](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial/intro).

## Configurations

 * Navigate to [**BLiP Portal**](https://portal.blip.ai).
 * Click in **Create chatbot** button and choose **Create from scratch** mode. If you already have your bot created just access them.
 * After your chatbot has been created click in **Configurations** and choose **Conection information** option in left side menu.
 * Enable the SDK connection and get the `identifier` and `access key` informations.
 * The identifier and the access key must be inserted in the `application.json` file of your project.

![Connection information](/Images/Connection.png)

Your `application.json` file must looks like:

	{
        "identifier": "your-identifier",
        "accessKey": "your-access-key",

        // other stuffs
	}

 * Create a `.json` file to your flash cards game. In this project, **the file name is** `people.json`.
 * **If you want** to change the name of `.json` file, **you need to change the** `Reader.cs`, **line 11**. You can change the class `People.cs` too.

![Reader](/Images/Reader.png)

The `<name-of-your-cards-file>.json` file must looks like:

	[
        {
            "Name": "<name>",
            "Uri": "<uri>",
            "PreviewUri": "<previewuri>"
        },
        {
            "Name": "<name>",
            "Uri": "<uri>",
            "PreviewUri": "<previewuri>"
        },

        // other objects
    ]

For more information about how to send images sharing a URL using the **Media Link** content type, access the [**BLiP Documentation**](https://docs.blip.ai/?csharp#images).

After setted connection informations, created your `application.json` and `<name-of-your-cards-file>.json` files, run the project. The console should show the following messages:

	Starting application...
	Application started. Press any key to stop.

See more information about how to build your application using the **BLiP SDK C#** at [**BLiP Documentation**](https://docs.blip.ai/?csharp#using-sdk-csharp).
