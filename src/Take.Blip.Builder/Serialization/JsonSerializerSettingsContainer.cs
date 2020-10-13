using System.Text.Json;

namespace Take.Blip.Builder.Serialization
{
    public static class JsonSerializerOptionsContainer
    {
        public static JsonSerializerOptions Settings { get; }

        static JsonSerializerOptionsContainer()
        {
            Settings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            };
        }
    }
}
