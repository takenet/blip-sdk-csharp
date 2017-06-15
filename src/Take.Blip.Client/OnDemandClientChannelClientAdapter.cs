using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Util;

namespace Take.Blip.Client
{
    public class OnDemandClientChannelClientAdapter : IClient
    {
        private static readonly TimeSpan ChannelDiscardedDelay = TimeSpan.FromSeconds(5);
        private bool _isStopping;

        private readonly IOnDemandClientChannel _onDemandClientChannel;

        public OnDemandClientChannelClientAdapter(IOnDemandClientChannel onDemandClientChannel)
        {
            _onDemandClientChannel = onDemandClientChannel;

            _onDemandClientChannel.ChannelCreatedHandlers.Add(ChannelCreatedAsync);
            _onDemandClientChannel.ChannelCreationFailedHandlers.Add(ChannelCreationFailedAsync);
            _onDemandClientChannel.ChannelDiscardedHandlers.Add(ChannelDiscardedAsync);
            _onDemandClientChannel.ChannelOperationFailedHandlers.Add(ChannelOperationFailedAsync);
        }

        public Task<Command> ProcessCommandAsync(Command command, CancellationToken cancellationToken = default(CancellationToken)) 
            => _onDemandClientChannel.ProcessCommandAsync(command, cancellationToken);

        public Task SendCommandResponseAsync(Command command, CancellationToken cancellationToken = default(CancellationToken))
            => _onDemandClientChannel.SendCommandAsync(command, cancellationToken);

        public Task SendMessageAsync(Message message, CancellationToken cancellationToken = default(CancellationToken))
            => _onDemandClientChannel.SendMessageAsync(message, cancellationToken);

        public Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default(CancellationToken))
            => _onDemandClientChannel.SendNotificationAsync(notification, cancellationToken);

        public Task StartAsync(CancellationToken cancellationToken)
            => _onDemandClientChannel.EstablishAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _isStopping = true;
            return _onDemandClientChannel.FinishAsync(cancellationToken);
        }

        private Task ChannelCreatedAsync(ChannelInformation channelInformation)
        {
            Trace.TraceInformation("Channel '{0}' created", channelInformation.SessionId);
            return Task.CompletedTask;
        }
        /// <summary>
        /// In this context, a LimeException usually means that some credential information is wrong, and should be checked.
        /// </summary>
        /// <param name="failedChannelInformation">Information about the failure</param>
        private async Task<bool> ChannelCreationFailedAsync(FailedChannelInformation failedChannelInformation)
        {
            Trace.TraceError("Channel '{0}' operation failed: {1}", failedChannelInformation.SessionId, failedChannelInformation.Exception);
            if (failedChannelInformation.Exception is LimeException) return false;
            await Task.Delay(ChannelDiscardedDelay).ConfigureAwait(false);
            return !_isStopping;
        }

        private Task ChannelDiscardedAsync(ChannelInformation channelInformation)
        {
            Trace.TraceInformation("Channel '{0}' discarded", channelInformation.SessionId);
            if (_isStopping) return Task.CompletedTask;
            return Task.Delay(ChannelDiscardedDelay);
        }

        private Task<bool> ChannelOperationFailedAsync(FailedChannelInformation failedChannelInformation)
        {
            Trace.TraceError("Channel '{0}' operation failed: {1}", failedChannelInformation.SessionId, failedChannelInformation.Exception);
            if (_isStopping) return TaskUtil.FalseCompletedTask;
            return TaskUtil.TrueCompletedTask;
        }
    }
}
