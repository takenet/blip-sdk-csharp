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

        /// <summary>
        /// The expiration for a user state in a flow.
        /// </summary>
        public TimeSpan? StateExpiration { get; set; }

        /// <summary>
        /// The minimum AI intent score to be considered valid.
        /// </summary>
        public double? MinimumIntentScore { get; set; }

        /// <summary>
        /// The global timeout for action execution. 
        /// </summary>
        public double? ActionExecutionTimeout { get; set; }

        /// <summary>
        /// Indicates that the commands should be processed as the owner of a tunnel (a router bot), if the input message is from a tunnel user.
        /// </summary>
        public bool? ProcessCommandsAsTunnelOwner { get; set; }
        
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