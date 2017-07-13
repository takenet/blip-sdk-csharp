using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Listeners;
using Lime.Protocol.Network;

namespace Take.Blip.Client
{
    public class BlipChannelListener : IBlipChannelListener, IDisposable
    {
        private readonly EnvelopeReceiverManager _envelopeReceiverManager;
        private readonly IChannelListener _channelListener;

        private CancellationTokenSource _cts;
        private object _syncRoot = new object();
        
        public BlipChannelListener(ISender sender, bool autoNotify)
        {
            _envelopeReceiverManager = new EnvelopeReceiverManager();

            var messageHandler = new MessageReceivedHandler(sender, autoNotify, _envelopeReceiverManager);
            var notificationHandler = new NotificationReceivedHandler(_envelopeReceiverManager);
            var commandHandler = new CommandReceivedHandler(sender, _envelopeReceiverManager);

            _channelListener = new ChannelListener(
                m => messageHandler.HandleAsync(m, _cts.Token),
                n => notificationHandler.HandleAsync(n, _cts.Token),
                c => commandHandler.HandleAsync(c, _cts.Token));
        }

        public Task<Message> MessageListenerTask => _channelListener.MessageListenerTask;

        public Task<Notification> NotificationListenerTask => _channelListener.NotificationListenerTask;

        public Task<Command> CommandListenerTask => _channelListener.CommandListenerTask;

        public void AddMessageReceiver(IMessageReceiver messageReceiver, Func<Message, Task<bool>> messageFilter, int priority = 0)
        {
            _envelopeReceiverManager.AddMessageReceiver(() => messageReceiver, messageFilter, priority);
        }

        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Func<Notification, Task<bool>> notificationFilter, int priority = 0)
        {
            _envelopeReceiverManager.AddNotificationReceiver(() => notificationReceiver, notificationFilter, priority);
        }

        public void AddCommandReceiver(ICommandReceiver commandReceiver, Func<Command, Task<bool>> commandFilter, int priority = 0)
        {
            _envelopeReceiverManager.AddCommandReceiver(() => commandReceiver, commandFilter, priority);
        }

        public void Start(IEstablishedReceiverChannel channel)
        {
            lock (_syncRoot)
            {
                if (_cts != null) throw new InvalidOperationException("The listener is already started");
                _cts = new CancellationTokenSource();
                _channelListener.Start(channel);
            }
        }

        public void Stop()
        {
            lock (_syncRoot)
            {
                if (_cts == null) throw new InvalidOperationException("The listener is not started");
                _channelListener.Stop();
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        public void Dispose()
        {
            _cts?.Dispose();
        }
    }
}
