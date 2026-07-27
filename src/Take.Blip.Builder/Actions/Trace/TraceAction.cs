using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Diagnostics;

namespace Take.Blip.Builder.Actions.Trace
{
    /// <summary>
    /// Builder action that appends a custom <see cref="UserTraceEntry"/> to the current
    /// <see cref="InputTrace.UserTraces"/> collection.
    ///
    /// Use this action in your flow to capture application-level diagnostic information
    /// at meaningful execution points — for example, recording an API response status,
    /// a business-rule checkpoint, or exception details — without relying on the
    /// analytics-oriented <c>TrackEvent</c> action.
    ///
    /// All settings fields support the standard <c>{{variableName}}</c> variable syntax,
    /// so values are resolved at runtime before the entry is recorded.
    ///
    /// The collected entries are included in the <c>userTraces</c> array of the
    /// <see cref="InputTrace"/> object that is forwarded to the configured trace target
    /// (HTTP endpoint or LIME node) at the end of the input processing cycle.
    /// When tracing is disabled the action is a no-op and completes immediately.
    /// </summary>
    public sealed class TraceAction : ActionBase<TraceActionSettings>
    {
        public TraceAction()
            : base(nameof(Trace)) { }

        /// <inheritdoc />
        public override Task ExecuteAsync(
            IContext context,
            TraceActionSettings settings,
            CancellationToken cancellationToken
        )
        {
            var entry = new UserTraceEntry
            {
                Name = settings.Name,
                Category = settings.Category,
                Value = settings.Value,
                Data = settings.Data,
                Timestamp = DateTimeOffset.UtcNow,
            };

            context.AddUserTrace(entry);

            return Task.CompletedTask;
        }
    }
}

