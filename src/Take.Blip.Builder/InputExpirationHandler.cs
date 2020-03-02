using Lime.Messaging.Contents;
using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Content;
using Take.Blip.Builder.Models;
using Take.Blip.Client.Content;
using Take.Blip.Client.Extensions.Scheduler;

namespace Take.Blip.Builder
{
    public class InputExpirationHandler : IInputExpirationHandler
    {
        private readonly Document _emptyContent = new PlainText() { Text = string.Empty };
        private readonly ISchedulerExtension _schedulerExtension;
        private string _stateIdInputExipiration = null;

        public InputExpirationHandler(ISchedulerExtension schedulerExtension)
        {
            _schedulerExtension = schedulerExtension;
        }

        public async Task OnFlowPreProcessingAsync(State state, Message message, Node from, CancellationToken cancellationToken)
        {
            // Cancel Schedule expiration time if input is configured
            if (state.HasInputExpiration())
            {
                await _schedulerExtension.CancelScheduledMessageAsync(message.GetInputExirationTimeIdMessage(), from, cancellationToken);
            }
        }

        public async Task OnFlowProcessedAsync(State state, Message message, Node from, CancellationToken cancellationToken)
        {
            // Schedule expiration time if input is configured
            if (state.HasInputExpiration())
            {
                await _schedulerExtension.ScheduleMessageAsync(message.CreateInputExirationTimeMessage(state.Id), DateTimeOffset.UtcNow.AddMinutes(state.Input.Expiration.Value.TotalMinutes), from, cancellationToken);
            }
        }

        public Message ValidadeMessage(Message message)
        {
            _stateIdInputExipiration = null;

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

                _stateIdInputExipiration = inputExpiration.StateId;

                return new Message(message.Id)
                {
                    To = message.To,
                    From = inputExpiration.Identity.ToNode(),
                    Content = _emptyContent
                };
            }

            return message;
        }

        public bool ValidadeState(State state)
        {
            return string.IsNullOrWhiteSpace(_stateIdInputExipiration) ||
                            state?.Id == _stateIdInputExipiration;
        }
    }
}
