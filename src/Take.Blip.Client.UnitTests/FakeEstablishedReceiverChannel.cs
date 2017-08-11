using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lime.Protocol;
using Lime.Protocol.Network;

namespace Take.Blip.Client.UnitTests
{
    public class FakeEstablishedReceiverChannel : IEstablishedReceiverChannel
    {        
        public BufferBlock<Message> MessageBuffer { get; } = new BufferBlock<Message>();


        public BufferBlock<Notification> NotificationBuffer { get; } = new BufferBlock<Notification>();

        public BufferBlock<Command> CommandBuffer { get; } = new BufferBlock<Command>();


        public Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken) 
            => MessageBuffer.ReceiveAsync<Message>(cancellationToken);

        public Task<Notification> ReceiveNotificationAsync(CancellationToken cancellationToken)
            => NotificationBuffer.ReceiveAsync<Notification>(cancellationToken);

        public Task<Command> ReceiveCommandAsync(CancellationToken cancellationToken) 
            => CommandBuffer.ReceiveAsync<Command>(cancellationToken);
    }
}