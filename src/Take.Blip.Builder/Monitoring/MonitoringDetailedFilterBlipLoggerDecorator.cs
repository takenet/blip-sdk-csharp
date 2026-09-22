using System;
using System.Collections.Generic;
using Blip.Ai.Bot.Monitoring.Logging.Interface;
using Blip.Ai.Bot.Monitoring.Logging.Models;

namespace Take.Blip.Builder.Monitoring
{
    /// <summary>
    /// A decorator for <see cref="IBlipLogger"/> that filters out certain log entries based on their title when detailed monitoring is disabled.
    /// </summary>
    public class MonitoringDetailedFilterBlipLoggerDecorator : IBlipLogger
    {
        private static readonly HashSet<string> _filteredTitles = new HashSet<string>(
            new[]
            {
                "TrackEvent",
                "ExecuteScript",
                "SetVariable",
                "ExecuteScriptV2",
                "ProcessHttp",
                "MergeContact",
                "Redirect",
                "ProcessCommand",
                "ManageList",
                "ProcessContentAssistant",
                "SendMessage",
                "SendRawMessage",
                "MaxTransitionsReached",
                "CommandInput",
                "StateProcessingStart",
                "LeavingFromDesk",
                "SendAgentMenssage",
                "ForwardToAgent",
                "LeavingFromAgent",
                "SubflowEntry",
                "SubflowReturn",
                "InputValidation",
                "InputProcessing",
            },
            StringComparer.OrdinalIgnoreCase
        );

        private readonly IBlipLogger _innerLogger;
        private readonly Func<bool> _isMonitoringDetailedEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitoringDetailedFilterBlipLoggerDecorator"/> class.
        /// </summary>
        /// <param name="innerLogger">The inner <see cref="IBlipLogger"/> to decorate.</param>
        /// <param name="isMonitoringDetailedEnabled">A function that indicates whether detailed monitoring is enabled.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="innerLogger"/> is null.</exception>
        public MonitoringDetailedFilterBlipLoggerDecorator(
            IBlipLogger innerLogger,
            Func<bool> isMonitoringDetailedEnabled
        )
        {
            _innerLogger = innerLogger ?? throw new ArgumentNullException(nameof(innerLogger));
            _isMonitoringDetailedEnabled = isMonitoringDetailedEnabled;
        }

        /// <summary>
        /// Processes a log message, forwarding it to the inner logger if it should not be suppressed based on the configuration and title.
        /// </summary>
        /// <param name="stateLog">The log input to process.</param>
        public void MessageProcessing(LogInput stateLog)
        {
            if (!ShouldSuppress(stateLog))
            {
                _innerLogger.MessageProcessing(stateLog);
            }
        }

        /// <summary>
        /// Processes an action execution log message, forwarding it to the inner logger if it should not be suppressed based on the configuration and title.
        /// </summary>
        /// <param name="stateLog">The log input to process.</param>
        public void ActionExecution(LogInput stateLog)
        {
            if (!ShouldSuppress(stateLog))
            {
                _innerLogger.ActionExecution(stateLog);
            }
        }

        /// <summary>
        /// Processes a user context log message, forwarding it to the inner logger if it should not be suppressed based on the configuration and title.
        /// </summary>
        /// <param name="stateLog">The log input to process.</param>
        public void UserContext(LogInput stateLog)
        {
            if (!ShouldSuppress(stateLog))
            {
                _innerLogger.UserContext(stateLog);
            }
        }

        /// <summary>
        /// Processes a conversational flow log message, forwarding it to the inner logger if it should not be suppressed based on the configuration and title.
        /// </summary>
        /// <param name="stateLog">The log input to process.</param>
        public void ConversationalFlow(LogInput stateLog)
        {
            if (!ShouldSuppress(stateLog))
            {
                _innerLogger.ConversationalFlow(stateLog);
            }
        }

        /// <summary>
        /// Processes a user input log message, forwarding it to the inner logger if it should not be suppressed based on the configuration and title.
        /// </summary>
        /// <param name="stateLog">The log input to process.</param>
        public void UserInput(LogInput stateLog)
        {
            if (!ShouldSuppress(stateLog))
            {
                _innerLogger.UserInput(stateLog);
            }
        }

        /// <summary>
        /// Processes a message delivery log message, forwarding it to the inner logger if it should not be suppressed based on the configuration and title.
        /// </summary>
        /// <param name="stateLog">The log input to process.</param>
        public void MessageDelivery(LogInput stateLog)
        {
            if (!ShouldSuppress(stateLog))
            {
                _innerLogger.MessageDelivery(stateLog);
            }
        }

        /// <summary>
        /// Processes a missing information latency log message, forwarding it to the inner logger if it should not be suppressed based on the configuration and title.
        /// </summary>
        /// <param name="stateLog">The log input to process.</param>
        public void MissingInfoLatency(LogInput stateLog)
        {
            if (!ShouldSuppress(stateLog))
            {
                _innerLogger.MissingInfoLatency(stateLog);
            }
        }

        /// <summary>
        /// Processes an error event log message, forwarding it to the inner logger regardless of the configuration and title.
        /// </summary>
        /// <param name="stateLog">The log input to process.</param>
        /// <param name="exception">The exception associated with the error event.</param>
        public void ErrorEvents(LogInput stateLog, Exception exception)
        {
            _innerLogger.ErrorEvents(stateLog, exception);
        }

        private bool ShouldSuppress(LogInput stateLog) =>
            !_isMonitoringDetailedEnabled()
            && !string.IsNullOrWhiteSpace(stateLog?.Title)
            && _filteredTitles.Contains(stateLog.Title);
    }
}
