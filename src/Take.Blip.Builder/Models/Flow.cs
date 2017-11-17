using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Models
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

        /// <summary>
        /// The flow variables. Optional.
        /// </summary>
        public Dictionary<string, string> Variables { get; set; }

        public void Validate()
        {
            Validator.ValidateObject(this, new ValidationContext(this));

            var rootState = States.SingleOrDefault(s => s.Root);
            if (rootState == null)
            {
                throw new ValidationException("The flow must have one root state");
            }

            if (rootState.Input == null || rootState.Input.Bypass)
            {
                throw new ValidationException("The root state must expect an input");
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
