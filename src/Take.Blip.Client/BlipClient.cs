using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Listeners;
using Lime.Protocol.Network;
using Lime.Protocol.Util;
using Serilog;

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
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore;

        private bool _isStopping;
        private IChannelListener _channelListener;

        public BlipClient(IOnDemandClientChannel onDemandClientChannel, ILogger logger = null)
        {
            _onDemandClientChannel = onDemandClientChannel ?? throw new ArgumentNullException(nameof(onDemandClientChannel));
            _logger = logger ?? LoggerProvider.Logger;
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
            catch (Exception ex)
            {
                _logger.Error(ex, "An error ocurred while starting the client: {Message}", ex.Message);
                throw;
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
            catch (Exception ex)
            {
                _logger.Error(ex, "An error ocurred while stopping the client: {Message}", ex.Message);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private Task ChannelCreatedAsync(ChannelInformation channelInformation)
        {   
            _logger.Information("Channel '{SessionId}' created - Local node {LocalNode} - Remote node: {RemoteNode}", 
                channelInformation.SessionId,
                channelInformation.LocalNode,
                channelInformation.RemoteNode);
            return Task.CompletedTask;
        }

        private async Task<bool> ChannelCreationFailedAsync(FailedChannelInformation failedChannelInformation)
        {
            _logger.Error(failedChannelInformation.Exception, "Channel '{SessionId}' creation failed - Local node: {LocalNode} - Remote node: {RemoteNode}", 
                failedChannelInformation.SessionId,
                failedChannelInformation.LocalNode,
                failedChannelInformation.RemoteNode);

            if (failedChannelInformation.Exception is LimeException ex && ex.Reason.Code == ReasonCodes.SESSION_AUTHENTICATION_FAILED) return false;
            await Task.Delay(ChannelDiscardedDelay).ConfigureAwait(false);
            return !_isStopping;
        }

        private Task ChannelDiscardedAsync(ChannelInformation channelInformation)
        {
            _logger.Information("Channel '{SessionId}' discarded - Local node: {LocalNode} - Remote node: {RemoteNode}",
                channelInformation.SessionId,
                channelInformation.LocalNode,
                channelInformation.RemoteNode);

            if (_isStopping) return Task.CompletedTask;
            return Task.Delay(ChannelDiscardedDelay);
        }

        private Task<bool> ChannelOperationFailedAsync(FailedChannelInformation failedChannelInformation)
        {
            _logger.Error(failedChannelInformation.Exception, "Channel '{SessionId}' operation '{OperationName}' failed - Local node: {LocalNode} - Remote node: {RemoteNode}",
                failedChannelInformation.SessionId,
                failedChannelInformation.OperationName,
                failedChannelInformation.LocalNode,
                failedChannelInformation.RemoteNode);

            if (_isStopping) return TaskUtil.FalseCompletedTask;
            return TaskUtil.TrueCompletedTask;
        }
    }
}
