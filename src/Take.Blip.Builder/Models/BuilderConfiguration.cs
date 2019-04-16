using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// A configuration container that holds specific configuration keys from builder.
    /// </summary>
    public class BuilderConfiguration
    {
        private const string CONFIGURATION_KEY_PREFIX = "builder:";

        public TimeSpan? StateExpiration { get; set; }

        public double? MinimumIntentScore { get; set; }

        public double? ActionExecutionTimeout { get; set; }
           
        public static BuilderConfiguration FromDictionary(IDictionary<string, string> configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            
            var builderConfiguration = configuration
                .Where(kv => kv.Key.StartsWith(CONFIGURATION_KEY_PREFIX))
                .Select(kv => new KeyValuePair<string, string>(kv.Key.Replace(CONFIGURATION_KEY_PREFIX, ""), kv.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            
            var jObject = JObject.FromObject(
                builderConfiguration, JsonSerializer.Create(JsonSerializerSettingsContainer.Settings));

            return jObject.ToObject<BuilderConfiguration>();
        }
    }
}