namespace  bot_flash_cards_blip_sdk_csharp
{
    using System;
    using Lime.Protocol;

    public class Person
    {
        public string Name { get; set; }

        public string Uri { get; set; }

        public string Title { get; set; }

        public string Text { get; set; } = "Who is this person?";

        public string AspectRatio { get; set; } = "1:1";

        public int Size { get; set; } = 227791;

        public string PreviewUri { get; set; }
    }
}