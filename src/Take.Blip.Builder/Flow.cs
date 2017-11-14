using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a conversational state machine.
    /// </summary>
    public class Flow : IFlow
    {
        public static JsonSerializerSettings SerializerSettings { get; }

        static Flow()
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

        /// <summary>
        /// The flow states. Required.
        /// </summary>
        public State[] States { get; set; }

        /// <summary>
        /// Creates an instance of <see cref="Flow"/> from a JSON input.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static Flow ParseFromJson(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            return JsonConvert.DeserializeObject<Flow>(json, SerializerSettings);
        }

        /// <summary>
        /// Creates an instance of <see cref="Flow" /> from a JSON file.
        /// </summary>
        /// <param name="filePath">The path.</param>
        /// <returns></returns>
        public static Flow ParseFromJsonFile(string filePath) => ParseFromJson(File.ReadAllText(filePath));
    }
}
