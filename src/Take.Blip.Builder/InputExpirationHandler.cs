using Lime.Messaging.Contents;
using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Models;
using Take.Blip.Client.Content;
using Take.Blip.Client.Extensions.Scheduler;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Handle the logic of input expiration
    /// </summary>
    public class InputExpirationHandler : IInputExpirationHandler
    {
        public const string STATE_ID = "inputExpiration.stateId";
        public const string IDENTITY = "inputExpiration.identity";
        private readonly Document _emptyContent = new PlainText() { Text = string.Empty };
        private readonly ISchedulerExtension _schedulerExtension;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="schedulerExtension"></param>
        public InputExpirationHandler(ISchedulerExtension schedulerExtension)
        {
            _schedulerExtension = schedulerExtension;
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
            // Cancel Schedule expiration time if input is configured
            if (state.HasInputExpiration() && !IsMessageFromExpiration(message))
            {
                var messageId = GetInputExirationIdMessage(message);
                await _schedulerExtension.CancelScheduledMessageAsync(messageId, from, cancellationToken);
            }
        }

        /// <summary>
        /// Execute after flow process
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="from"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task OnFlowProcessedAsync(State state, Message message, Node from, CancellationToken cancellationToken)
        {
            // Schedule expiration time if input is configured
            if (state.HasInputExpiration())
            {
                var scheduleMessage = CreateInputExirationMessage(message, state.Id);
                var scheduleTime = DateTimeOffset.UtcNow.AddMinutes(state.Input.Expiration.Value.TotalMinutes);
                await _schedulerExtension.ScheduleMessageAsync(scheduleMessage,  scheduleTime, from, cancellationToken);
            }
        }

        /// <summary>
        /// Check and change message if needs
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Message ValidateMessage(Message message)
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

                return new Message(message.Id)
                {
                    To = message.To,
                    From = inputExpiration.Identity.ToNode(),
                    Content = _emptyContent,
                    Metadata = messageMetadata
                };
            }

            return message;
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
        /// <returns></returns>
        public bool IsValidateState(State state, Message message)
        {
            string stateToGo = string.Empty;

            return message?.Metadata?.TryGetValue(STATE_ID, out stateToGo) != true ||
                            string.IsNullOrEmpty(stateToGo) ||
                            state?.Id == stateToGo;
        }

        private Message CreateInputExirationMessage(Message message, string stateId)
        {
            var idMessage = GetInputExirationIdMessage(message);

            TraceSettings traceSettings = GetTraceSettings(message);

            return new Message(idMessage)
            {
                To = message.To,
                Content = new InputExpiration()
                {
                    Identity = message.From,
                    StateId = stateId
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
    }
}
