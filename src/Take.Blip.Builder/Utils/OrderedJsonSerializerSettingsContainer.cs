using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Take.Blip.Builder.Utils
{
    public static class OrderedJsonSerializerSettingsContainer
    {
        public static JsonSerializerSettings Settings { get; }

        static OrderedJsonSerializerSettingsContainer()
        {
            Settings = new JsonSerializerSettings
            {
                ContractResolver = new OrderedCamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
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
