using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Take.Blip.Builder.Utils
{
    public static class JsonSerializerSettingsContainer
    {
        public static JsonSerializerSettings SerializerSettings { get; }

        static JsonSerializerSettingsContainer()
        {
            SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            SerializerSettings.Converters.Add(
                new StringEnumConverter
                {
                    CamelCaseText = true,
                    AllowIntegerValues = true
                });
        }
    }
}
