using System.Collections.Generic;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// ContentAssistant resposne object
    /// </summary>
    public class ContentAssistantActionResponse
    {
        /// <summary>
        /// Bool that indicates if there's a combination found
        /// </summary>
        public bool HasCombination { get; set; }

        /// <summary>
        /// Response found for combination
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// IntentFound
        /// </summary>
        public string Intent { get; set; }

        /// <summary>
        /// Entities found
        /// </summary>
        public List<string> Entities { get; set; }
    }
}
