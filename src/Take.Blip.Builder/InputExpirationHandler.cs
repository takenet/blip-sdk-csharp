using Lime.Messaging.Contents;
using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        private const string STATE_ID = "inputExpiration.stateId";
        private const string IDENTITY = "inputExpiration.identity";
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
            if (state.HasInputExpiration())
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

                return new Message(message.Id)
                {
                    To = message.To,
                    From = inputExpiration.Identity.ToNode(),
                    Content = _emptyContent,
                    Metadata = new Dictionary<string,string>()
                    {
                        { STATE_ID, inputExpiration.StateId },
                        { IDENTITY, inputExpiration.Identity },
                    }
                };
            }

            return message;
        }

        /// <summary>
        /// Check is state is valid
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool ValidateState(State state, Message message)
        {
            string stateToGo = string.Empty;

            return message?.Metadata?.TryGetValue(STATE_ID, out stateToGo) != true ||
                            string.IsNullOrEmpty(stateToGo) ||
                            state?.Id == stateToGo;
        }

        private Message CreateInputExirationMessage(Message message, string stateId)
        {
            var idMessage = GetInputExirationIdMessage(message);

            return new Message(idMessage)
            {
                To = message.To,
                Content = new InputExpiration()
                {
                    Identity = message.From,
                    StateId = stateId
                }
            };
        }

        private string GetInputExirationIdMessage(Message message)
        {
            return $"{message.From.ToIdentity()}-inputexpirationtime";
        }
    }
}
