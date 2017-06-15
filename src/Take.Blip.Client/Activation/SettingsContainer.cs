using System.Collections.Generic;

namespace Take.Blip.Client.Activation
{
    public class SettingsContainer
    {
        /// <summary>
        /// Gets or sets the settings to be injected to the startup and receivers types.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public IDictionary<string, object> Settings { get; set; }

        /// <summary>
        /// Gets or sets a type to be used to deserialize the settings property.
        /// </summary>
        public string SettingsType { get; set; }
    }
}
