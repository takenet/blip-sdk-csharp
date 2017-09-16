using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Take.Blip.Client.Activation;

namespace Take.Blip.Client.TestKit
{
    internal class InternalOnDemandClientChannel : IOnDemandClientChannel
    {
        private readonly IEnvelopeSerializer _serializer;
        private readonly Application _applicationSettings;

        internal BufferBlock<Message> IncomingMessages { get; }

        internal BufferBlock<Command> IncomingCommands { get; }

        internal BufferBlock<Notification> IncomingNotifications { get; }

        internal BufferBlock<Message> OutgoingMessages { get; }

        internal BufferBlock<Command> OutgoingCommands { get; }

        internal BufferBlock<Notification> OutgoingNotifications { get; }

        public InternalOnDemandClientChannel(IEnvelopeSerializer serializer, Application applicationSettings)
        {
            _applicationSettings = applicationSettings;
            _serializer = serializer;

            IncomingMessages = new BufferBlock<Message>();
            IncomingCommands = new BufferBlock<Command>();
            IncomingNotifications = new BufferBlock<Notification>();

            OutgoingMessages = new BufferBlock<Message>();
            OutgoingCommands = new BufferBlock<Command>();
            OutgoingNotifications = new BufferBlock<Notification>();
        }

        public ICollection<Func<ChannelInformation, Task>> ChannelCreatedHandlers => throw new NotImplementedException();

        public ICollection<Func<FailedChannelInformation, Task<bool>>> ChannelCreationFailedHandlers => throw new NotImplementedException();

        public ICollection<Func<ChannelInformation, Task>> ChannelDiscardedHandlers => throw new NotImplementedException();

        public ICollection<Func<FailedChannelInformation, Task<bool>>> ChannelOperationFailedHandlers => throw new NotImplementedException();

        public bool IsEstablished { get; private set; }

        public Task EstablishAsync(CancellationToken cancellationToken)
        {
            IsEstablished = true;
            return Task.CompletedTask;
        }

        public Task FinishAsync(CancellationToken cancellationToken)
        {
            IsEstablished = false;
            return Task.CompletedTask;
        }

        public Task<Command> ReceiveCommandAsync(CancellationToken cancellationToken)
        {
            return IncomingCommands.ReceiveAsync(cancellationToken);
        }

        public Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            return IncomingMessages.ReceiveAsync(cancellationToken);
        }

        public Task<Notification> ReceiveNotificationAsync(CancellationToken cancellationToken)
        {
            return IncomingNotifications.ReceiveAsync(cancellationToken);
        }

        public Task<Command> ProcessCommandAsync(Command command, CancellationToken cancellationToken)
        {
            var successResult = new Command
            {
                Id = command.Id,
                Method = command.Method,
                Status = CommandStatus.Success
            };
            return Task.FromResult(successResult);
        }

        public Task SendCommandAsync(Command command, CancellationToken cancellationToken)
            => OutgoingCommands.SendAsync(command);

        public Task SendMessageAsync(Message message, CancellationToken cancellationToken)
            => OutgoingMessages.SendAsync(message);

        public Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken)
            => OutgoingNotifications.SendAsync(notification);
    }
}
