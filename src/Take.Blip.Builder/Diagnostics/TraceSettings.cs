using System;
using System.Collections.Generic;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Diagnostics
{
    public class TraceSettings : IValidable
    {
        private const string BuilderTraceMode = "builder.trace.mode";
        private const string BuilderTraceTargetType = "builder.trace.targetType";
        private const string BuilderTraceTarget = "builder.trace.target";
        private const string BuilderTraceSlowthreshold = "builder.trace.slowThreshold";

        public TraceSettings()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="TraceSettings"/> based on the message metadata.
        /// Mode, TargetType and Target required
        /// SlowThreshold optional
        /// </summary>
        /// <param name="messageSettings"></param>
        public TraceSettings(IDictionary<string, string> messageSettings)
        {
            if (messageSettings == null ||
                !messageSettings.Keys.Contains(BuilderTraceMode) ||
                !messageSettings.Keys.Contains(BuilderTraceTargetType) ||
                !messageSettings.Keys.Contains(BuilderTraceTarget))
            {
                throw new ArgumentException();
            }
            Enum.TryParse<TraceMode>(messageSettings[BuilderTraceMode], out var traceMode);
            Mode = traceMode;

            Enum.TryParse<TraceTargetType>(messageSettings[BuilderTraceTargetType], out var targetType);
            TargetType = targetType;

            Target = messageSettings[BuilderTraceTarget];

            if (messageSettings.Keys.Contains(BuilderTraceSlowthreshold))
            {
                int.TryParse(messageSettings[BuilderTraceSlowthreshold], out var slowThreshold);
                SlowThreshold = slowThreshold;
            }
        }

        public TraceMode Mode { get; set; }

        public TraceTargetType TargetType { get; set; }

        public string Target { get; set; }

        public int? SlowThreshold { get; set; }

        public void Validate()
        {
            if (Mode == TraceMode.Disabled) return;
        }
    }
}