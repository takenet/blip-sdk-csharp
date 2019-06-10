using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Lime.Protocol;
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
        /// The application identity to be used as owner on commands with the BLiP API.
        /// If not provided, the value is determined from the input message.
        /// </summary>
        public Identity Application { get; set; }
        
        /// <summary>
        /// Indicates that the tunnel owner should be used as application if the input message is from a tunnel user.
        /// </summary>
        public bool? UseTunnelOwnerAsApplication { get; set; }        
        
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