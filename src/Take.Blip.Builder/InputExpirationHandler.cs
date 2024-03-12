using Lime.Messaging.Contents;
using Lime.Protocol;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Storage;
using Take.Blip.Client;
using Take.Blip.Client.Content;
using Take.Blip.Client.Extensions.Scheduler;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Handle the logic of input expiration
    /// </summary>
    public class InputExpirationHandler : IInputMessageHandler
    {
        public const string STATE_ID = "inputExpiration.stateId";
        public const string IDENTITY = "inputExpiration.identity";
        public const string CURRENT_SESSION_STATE = "inputExpiration.currentSessionState";
        private const string IS_INPUT_EXPIRATION_FROM_SUBFLOW_REDIRECT = "isInputExpirationFromSubflowRedirect";

        private readonly Document _emptyContent = new PlainText() { Text = string.Empty };
        private readonly ISchedulerExtension _schedulerExtension;
        private readonly ILogger _logger;
        private readonly IInputExpirationCount _inputExpirationCount;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="schedulerExtension"></param>
        public InputExpirationHandler(ISchedulerExtension schedulerExtension, ILogger logger, IInputExpirationCount inputExpirationCount, IStateManager stateManager, IConfiguration configuration)
        {
            _schedulerExtension = schedulerExtension;
            _logger = logger;
            _inputExpirationCount = inputExpirationCount;
            _configuration = configuration;
        }

        /// <summary>
        /// Execute before flow process
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="from"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task OnFlowPreProcessingAsync(State state, Message message, Node from, CancellationToken cancellationToken)
        {
            if (!IsMessageFromExpiration(message))
            {
                _inputExpirationCount.TryRemoveAsync(message);
            }

            // Cancel Schedule expiration time if input is configured
            if (state.HasInputExpiration() && !IsMessageFromExpiration(message))
            {
                var messageId = GetInputExirationIdMessage(message);

                Schedule scheduledMessage = null;

                try
                {
                    scheduledMessage = await _schedulerExtension.GetScheduledMessageAsync(messageId, from, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Scheduled message with id '{MessageId}' not scheduled", messageId);
                }

                if (scheduledMessage != null)
                {
                    await _schedulerExtension.CancelScheduledMessageAsync(messageId, from, cancellationToken);
                }

            }
        }

        /// <summary>
        /// Execute after flow process
        /// </summary>
        /// <param name="state"></param>
        /// <param name="flow"></param>
        /// <param name="message"></param>
        /// <param name="from"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task OnFlowProcessedAsync(State state, Flow flow, Message message, Node from, IContext context, CancellationToken cancellationToken)
        {

            // Schedule expiration time if input is configured
            if (state.HasInputExpiration() && await ValidateInputExirationCountAsync(state, message, from, context, cancellationToken))
            {
                await ScheduleInputExpirationAsync(state, flow, message, from, context, cancellationToken);
            }
        }

        /// <summary>
        /// Check and change message if needs
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public (bool MessageHasChanged, Message NewMessage) HandleMessage(Message message)
        {

            if (message.Content is InputExpiration inputExpiration)
            {

                if (string.IsNullOrWhiteSpace(inputExpiration?.Identity?.ToString()))
                {
                    throw new ArgumentException("Message content 'Identity' must be present", nameof(InputExpiration.Identity));
                }

                if (string.IsNullOrWhiteSpace(inputExpiration?.StateId))
                {
                    throw new ArgumentException("Message content 'StateId' must be present", nameof(InputExpiration.StateId));
                }

                var messageMetadata = GetTraceSettings(message)?.GetDictionary() ?? new Dictionary<string, string>();
                messageMetadata.Add(STATE_ID, inputExpiration.StateId);
                messageMetadata.Add(IDENTITY, inputExpiration.Identity);
                messageMetadata.Add(CURRENT_SESSION_STATE, inputExpiration.CurrentSessionState);

                message = new Message(message.Id)
                {
                    To = message.To,
                    From = inputExpiration.Identity.ToNode(),
                    Content = _emptyContent,
                    Metadata = messageMetadata
                };

                return (true, message);
            }

            return (false, message);
        }

        private bool IsMessageFromExpiration(Message message)
        {
            return message.Metadata?.ContainsKey(STATE_ID) ?? false;
        }

        /// <summary>
        /// Check is state is valid
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="flow"></param>
        /// <returns></returns>
        public bool IsValidateState(State state, Message message, Flow flow)
        {
            string stateToGo = string.Empty;
            string currentSessionState = string.Empty;

            return message?.Metadata?.TryGetValue(STATE_ID, out stateToGo) != true ||
                            string.IsNullOrEmpty(stateToGo) ||
                            stateToGo == IS_INPUT_EXPIRATION_FROM_SUBFLOW_REDIRECT ||
                           ((state?.Id == stateToGo) && message?.Metadata?.TryGetValue(CURRENT_SESSION_STATE, out currentSessionState) != true) ||
                            string.IsNullOrEmpty(stateToGo) ||
                            stateToGo == IS_INPUT_EXPIRATION_FROM_SUBFLOW_REDIRECT ||
                            (flow?.SessionState == currentSessionState);
        }

        private Message CreateInputExirationMessage(Message message, string stateId, string currentSessionState)
        {
            var idMessage = GetInputExirationIdMessage(message);

            TraceSettings traceSettings = GetTraceSettings(message);

            return new Message(idMessage)
            {
                To = message.To,
                Content = new InputExpiration()
                {
                    Identity = message.From,
                    StateId = stateId,
                    CurrentSessionState = currentSessionState
                },
                Metadata = traceSettings?.GetDictionary()
            };
        }

        private static TraceSettings GetTraceSettings(Message message)
        {
            if (message.Metadata != null &&
                message.Metadata.Keys.Contains(TraceSettings.BUILDER_TRACE_TARGET))
            {
                return new TraceSettings(message.Metadata);
            }

            return null;
        }

        private string GetInputExirationIdMessage(Message message)
        {
            return $"{message.From.ToIdentity()}-inputexpirationtime";
        }

        private async Task ScheduleInputExpirationAsync(State state, Flow flow, Message message, Node from, IContext context, CancellationToken cancellationToken)
        {
            var scheduleMessage = CreateInputExirationMessage(message, state.Id, flow.SessionState);
            var scheduleTime = DateTimeOffset.UtcNow.AddMinutes(state.Input.Expiration.Value.TotalMinutes);
            await _schedulerExtension.ScheduleMessageAsync(scheduleMessage, scheduleTime, from, cancellationToken);
        }
        private async Task<bool> ValidateInputExirationCountAsync(State state, Message message, Node from, IContext context, CancellationToken cancellationToken)
        {
            message.Metadata.TryGetValue(STATE_ID, out string stateIdInputExpiration);
            if (stateIdInputExpiration != null && stateIdInputExpiration == state.Id)
            {
                var inputExpirationCount = await _inputExpirationCount.IncrementAsync(message);
                if (inputExpirationCount > _configuration.MaximumInputExpirationLoop)
                {
                    await _inputExpirationCount.TryRemoveAsync(message);
                    _logger.Warning("[{Source}] [FlowConstruction] Max input expiration transitions of {MaximumInputExpirationLoop} was reached",
                     nameof(InputExpirationHandler), _configuration.MaximumInputExpirationLoop);
                    return false;
                }
            }
            return true;
        }
    }
}
