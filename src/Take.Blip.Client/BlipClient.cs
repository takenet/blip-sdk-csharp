using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Listeners;
using Lime.Protocol.Network;
using Lime.Protocol.Util;

namespace Take.Blip.Client
{
    /// <summary>
    /// Implements a client for BLiP connections.
    /// For creating new instances, use the <see cref="BlipClientBuilder"/> class.
    /// </summary>
    public class BlipClient : IBlipClient
    {
        private static readonly TimeSpan ChannelDiscardedDelay = TimeSpan.FromSeconds(5);
        
        private readonly IOnDemandClientChannel _onDemandClientChannel;
        private readonly SemaphoreSlim _semaphore;

        private bool _isStopping;
        private IChannelListener _channelListener;

        public BlipClient(IOnDemandClientChannel onDemandClientChannel)
        {
            _onDemandClientChannel = onDemandClientChannel ?? throw new ArgumentNullException(nameof(onDemandClientChannel));
            _semaphore = new SemaphoreSlim(1, 1);
            _onDemandClientChannel.ChannelCreatedHandlers.Add(ChannelCreatedAsync);
            _onDemandClientChannel.ChannelCreationFailedHandlers.Add(ChannelCreationFailedAsync);
            _onDemandClientChannel.ChannelDiscardedHandlers.Add(ChannelDiscardedAsync);
            _onDemandClientChannel.ChannelOperationFailedHandlers.Add(ChannelOperationFailedAsync);
        }

        public Task<Command> ProcessCommandAsync(Command command, CancellationToken cancellationToken)
            => _onDemandClientChannel.ProcessCommandAsync(command, cancellationToken);

        public Task SendCommandAsync(Command command, CancellationToken cancellationToken)
            => _onDemandClientChannel.SendCommandAsync(command, cancellationToken);

        public Task SendMessageAsync(Message message, CancellationToken cancellationToken)
            => _onDemandClientChannel.SendMessageAsync(message, cancellationToken);

        public Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken)
            => _onDemandClientChannel.SendNotificationAsync(notification, cancellationToken);

        public async Task StartAsync(IChannelListener channelListener, CancellationToken cancellationToken)
        {
            if (_channelListener != null) throw new InvalidOperationException("The client is already started");

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _channelListener = channelListener ?? throw new ArgumentNullException(nameof(channelListener));                
                _channelListener.Start(_onDemandClientChannel);
                await _onDemandClientChannel.EstablishAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channelListener == null) throw new InvalidOperationException("The client is not started");

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _isStopping = true;
                _channelListener?.Stop();
                await _onDemandClientChannel.FinishAsync(cancellationToken).ConfigureAwait(false);
                _channelListener = null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private Task ChannelCreatedAsync(ChannelInformation channelInformation)
        {
            Trace.TraceInformation("Channel '{0}' created", channelInformation.SessionId);
            return Task.CompletedTask;
        }

        private async Task<bool> ChannelCreationFailedAsync(FailedChannelInformation failedChannelInformation)
        {
            Trace.TraceError("Channel '{0}' operation failed: {1}", failedChannelInformation.SessionId, failedChannelInformation.Exception);
            if (failedChannelInformation.Exception is LimeException ex && ex.Reason.Code == ReasonCodes.SESSION_AUTHENTICATION_FAILED) return false;
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
