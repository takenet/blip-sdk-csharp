using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Take.Blip.Builder.Utils
{
    public static class JsonSerializerSettingsContainer
    {
        public static JsonSerializerSettings Settings { get; }

        static JsonSerializerSettingsContainer()
        {
            Settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            Settings.Converters.Add(
                new StringEnumConverter
                {
                    CamelCaseText = true,
                    AllowIntegerValues = true
                });
        }
    }
}
