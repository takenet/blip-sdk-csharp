using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Lime.Protocol;

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
        /// If not provided, is used the local application identity.
        /// </summary>
        public Identity OwnerIdentity { get; set; }
        
        /// <summary>
        /// Indicates that the tunnel owner context should be used if the input message is from a tunnel user.
        /// </summary>
        public bool? UseTunnelOwnerContext { get; set; }        
        
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

            var builderConfiguration = new BuilderConfiguration();
            var builderConfigurationType = typeof(BuilderConfiguration);

            configuration
                .Where(kv => kv.Key.StartsWith(CONFIGURATION_KEY_PREFIX))
                .ForEach(kv =>
                {
                    var (key, value) = kv;

                    var propertyName = key.Replace(CONFIGURATION_KEY_PREFIX, string.Empty).ToTitleCase();
                    var property = builderConfigurationType.GetProperty(propertyName);

                    var typeConverter = TypeDescriptor.GetConverter(property.PropertyType);
                    property?.SetValue(builderConfiguration, typeConverter.ConvertFromString(value));
                });

            return builderConfiguration;
        }
    }
}