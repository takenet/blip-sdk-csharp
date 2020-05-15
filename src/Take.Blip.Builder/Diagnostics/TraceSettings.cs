using System;
using System.Collections.Generic;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Diagnostics
{
    public class TraceSettings : IValidable
    {
        public const string BUILDER_TRACE_MODE = "builder.trace.mode";
        public const string BUILDER_TRACE_TARGET_TYPE = "builder.trace.targetType";
        public const string BUILDER_TRACE_TARGET = "builder.trace.target";
        public const string BUILDER_TRACE_SLOW_THRESHOLD = "builder.trace.slowThreshold";

        public TraceSettings()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="TraceSettings"/> based on the message metadata.
        /// Mode, TargetType and Target required
        /// SlowThreshold optional
        /// </summary>
        /// <param name="messageMetadata"></param>
        public TraceSettings(IDictionary<string, string> messageMetadata)
        {
            if (messageMetadata == null ||
                !(messageMetadata.TryGetValue(BUILDER_TRACE_MODE, out var traceModeString) && Enum.TryParse<TraceMode>(traceModeString, out var traceMode)) ||
                !(messageMetadata.TryGetValue(BUILDER_TRACE_TARGET_TYPE, out var traceTargeTypeString) && Enum.TryParse<TraceTargetType>(traceTargeTypeString, out var targetType)) ||
                !(messageMetadata.TryGetValue(BUILDER_TRACE_TARGET, out var target)))
            {
                throw new ArgumentException(nameof(messageMetadata));
            }
            Mode = traceMode;
            Target = target;
            TargetType = targetType;

            if (messageMetadata.TryGetValue(BUILDER_TRACE_SLOW_THRESHOLD, out var slowThresholdString))
            {
                int.TryParse(slowThresholdString, out var slowThreshold);
                SlowThreshold = slowThreshold;
            }
        }

        public TraceMode Mode { get; set; }

        public TraceTargetType TargetType { get; set; }

        public string Target { get; set; }

        public int? SlowThreshold { get; set; }

        public IDictionary<string,string> GetDictionary()
        {
            var returnDictionary = new Dictionary<string, string>()
            {
                { BUILDER_TRACE_MODE, Mode.ToString() },
                { BUILDER_TRACE_TARGET, Target },
                { BUILDER_TRACE_TARGET_TYPE, TargetType.ToString() }
            };

            if (SlowThreshold != null)
            {
                returnDictionary.Add(BUILDER_TRACE_SLOW_THRESHOLD, SlowThreshold.ToString());
            }

            return returnDictionary;
        }

        public void Validate()
        {
            if (Mode == TraceMode.Disabled) return;
        }
    }
}