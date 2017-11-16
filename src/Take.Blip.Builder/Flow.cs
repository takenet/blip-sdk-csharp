using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a conversational state machine.
    /// </summary>
    public class Flow
    {
        /// <summary>
        /// The unique identifier of the flow. Required.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// The flow states. Required.
        /// </summary>
        [Required]
        public State[] States { get; set; }

        public void Validate()
        {
            Validator.ValidateObject(this, new ValidationContext(this));

            if (States.Count(s => s.Root) != 1)
            {
                throw new ValidationException("The flow must have one root state");
            }
        }


        /// <summary>
        /// Creates an instance of <see cref="Flow"/> from a JSON input.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static Flow ParseFromJson(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            return JsonConvert.DeserializeObject<Flow>(json, JsonSerializerSettingsContainer.Settings);
        }

        /// <summary>
        /// Creates an instance of <see cref="Flow" /> from a JSON file.
        /// </summary>
        /// <param name="filePath">The path.</param>
        /// <returns></returns>
        public static Flow ParseFromJsonFile(string filePath) => ParseFromJson(File.ReadAllText(filePath));
    }
}
