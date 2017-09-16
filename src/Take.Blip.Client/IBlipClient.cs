using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Listeners;
using Lime.Protocol.Server;
using Lime.Protocol.Util;

namespace Take.Blip.Client
{
    /// <summary>
    /// Defines a BLiP client service.
    /// </summary>
    public interface IBlipClient : ISender, IStoppable
    {
        /// <summary>
        /// Starts the client with the specified listener instance.
        /// </summary>
        /// <param name="channelListener">The listener for consuming received envelopes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task StartAsync(IChannelListener channelListener, CancellationToken cancellationToken);
    }

    public static class ClientExtensions
    {
        /// <summary>
        /// Starts the client with no envelope consumers.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task StartAsync(this IBlipClient client, CancellationToken cancellationToken)
            => client.StartAsync(
                m => TaskUtil.TrueCompletedTask,
                n => TaskUtil.TrueCompletedTask,
                c => TaskUtil.TrueCompletedTask,
                cancellationToken);

        /// <summary>
        /// Starts the client with the specified envelope consumers.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="messageConsumer">The consumer func for received messages.</param>
        /// <param name="notificationConsumer">The consumer func for received notifications.</param>
        /// <param name="commandConsumer">The consumer func for received commands.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task StartAsync(
            this IBlipClient client,
            Func<Message, Task<bool>> messageConsumer,
            Func<Notification, Task<bool>> notificationConsumer,
            Func<Command, Task<bool>> commandConsumer,
            CancellationToken cancellationToken) 
                => client.StartAsync(
                    new ChannelListener(messageConsumer, notificationConsumer, commandConsumer),
                    cancellationToken);
    }
}
