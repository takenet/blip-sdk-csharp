using System;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.ProcessContentAssistant
{
    /// <summary>
    /// Content
    /// </summary>
    public class ProcessContentAssistantSettings : IValidable
    {
        /// <summary>
        /// Sentence to be analyzed
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Output variable
        /// </summary>
        public string OutputVariable { get; set; }

        /// <summary>
        /// Filter response
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// Enable content V2
        /// </summary>
        public bool V2 { get; set; }

        /// <summary>
        /// Minimum intent score
        /// </summary>
        public double? Score { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Text))
            {
                throw new ArgumentException($"The '{nameof(Text)}' settings value is required for '{nameof(ProcessContentAssistantSettings)}' action");
            }
            if (string.IsNullOrEmpty(OutputVariable))
            {
                throw new ArgumentException($"The '{nameof(OutputVariable)}' settings value is required for '{nameof(ProcessContentAssistantSettings)}' action");
            }
        }
    }
}