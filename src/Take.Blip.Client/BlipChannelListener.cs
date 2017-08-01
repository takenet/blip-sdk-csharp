using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lime.Protocol;
using Lime.Protocol.Listeners;
using Lime.Protocol.Network;
using Take.Blip.Client.Receivers;

namespace Take.Blip.Client
{
    public class BlipChannelListener : IBlipChannelListener, IDisposable
    {
        private readonly ISender _sender;
        private readonly bool _autoNotify;

        private readonly IChannelListener _channelListener;
        private readonly IList<ReceiverFactoryPredicate<Message>> _messageReceivers;
        private readonly IList<ReceiverFactoryPredicate<Notification>> _notificationReceivers;
        private readonly IList<ReceiverFactoryPredicate<Command>> _commandReceivers;

        private readonly ActionBlock<Message> _messageActionBlock;
        private readonly ActionBlock<Notification> _notificationActionBlock;
        private readonly ActionBlock<Command> _commandActionBlock;

        private CancellationTokenSource _cts;
        private readonly object _syncRoot = new object();
        
        public BlipChannelListener(ISender sender, bool autoNotify)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _autoNotify = autoNotify;

            _messageReceivers = new List<ReceiverFactoryPredicate<Message>>(new[]
            {
                new ReceiverFactoryPredicate<Message>(() => new UnsupportedMessageReceiver(), m => Task.FromResult(true), int.MaxValue)
            });
            _notificationReceivers = new List<ReceiverFactoryPredicate<Notification>>(new[]
            {
                new ReceiverFactoryPredicate<Notification>(() => new BlackholeEnvelopeReceiver(), n => Task.FromResult(true), int.MaxValue)
            });
            _commandReceivers = new List<ReceiverFactoryPredicate<Command>>(new[]
            {
                new ReceiverFactoryPredicate<Command>(() => new UnsupportedCommandReceiver(), c => Task.FromResult(true), int.MaxValue)
            });

            var dataflowBlockOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
                BoundedCapacity = DataflowBlockOptions.Unbounded
            };

            _messageActionBlock = new ActionBlock<Message>(
                m => HandleMessageAsync(m, _cts.Token));
            _notificationActionBlock = new ActionBlock<Notification>(
                m => HandleNotificationAsync(m, _cts.Token));
            _commandActionBlock = new ActionBlock<Command>(
                m => HandleCommandAsync(m, _cts.Token));
            _channelListener = new DataflowChannelListener(
                _messageActionBlock,
                _notificationActionBlock,
                _commandActionBlock);
        }

        public Task<Message> MessageListenerTask => _channelListener.MessageListenerTask;

        public Task<Notification> NotificationListenerTask => _channelListener.NotificationListenerTask;

        public Task<Command> CommandListenerTask => _channelListener.CommandListenerTask;

        public void AddMessageReceiver(IMessageReceiver messageReceiver, Func<Message, Task<bool>> messageFilter, int priority = 0)
            => AddEnvelopeReceiver(_messageReceivers, () => messageReceiver, messageFilter, priority);

        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Func<Notification, Task<bool>> notificationFilter, int priority = 0)
            => AddEnvelopeReceiver(_notificationReceivers, () => notificationReceiver, notificationFilter, priority);

        public void AddCommandReceiver(ICommandReceiver commandReceiver, Func<Command, Task<bool>> commandFilter, int priority = 0)
            => AddEnvelopeReceiver(_commandReceivers, () => commandReceiver, commandFilter, priority);

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

                _messageActionBlock.Complete();
                _notificationActionBlock.Complete();
                _commandActionBlock.Complete();

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

        private static void AddEnvelopeReceiver<T>(
            IList<ReceiverFactoryPredicate<T>> envelopeReceivers,
            Func<IEnvelopeReceiver<T>> receiverFactory,
            Func<T, Task<bool>> predicate,
            int priority) where T : Envelope, new()
        {
            if (receiverFactory == null) throw new ArgumentNullException(nameof(receiverFactory));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var predicateReceiverFactory = new ReceiverFactoryPredicate<T>(receiverFactory, predicate, priority);
            envelopeReceivers.Add(predicateReceiverFactory);
        }

        private async Task<IEnumerable<ReceiverFactoryPredicate<TEnvelope>>> GetReceiversAsync<TEnvelope>(TEnvelope envelope)
            where TEnvelope : Envelope, new()
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));

            if (envelope is Message)
            {
                return (IEnumerable<ReceiverFactoryPredicate<TEnvelope>>)
                    await FilterReceivers(_messageReceivers, envelope as Message);
            }

            if (envelope is Notification)
            {
                return (IEnumerable<ReceiverFactoryPredicate<TEnvelope>>)
                    await FilterReceivers(_notificationReceivers, envelope as Notification);
            }

            if (envelope is Command)
            {
                return (IEnumerable<ReceiverFactoryPredicate<TEnvelope>>)
                    await FilterReceivers(_commandReceivers, envelope as Command);
            }

            return Enumerable.Empty<ReceiverFactoryPredicate<TEnvelope>>();
        }

        private static async Task<IEnumerable<ReceiverFactoryPredicate<TEnvelope>>> FilterReceivers<TEnvelope>(
            IEnumerable<ReceiverFactoryPredicate<TEnvelope>> envelopeReceivers,
            TEnvelope envelope)
            where TEnvelope : Envelope, new()
        {
            var result = new List<ReceiverFactoryPredicate<TEnvelope>>();
            foreach (var receiver in envelopeReceivers)
            {
                if (await receiver.Predicate(envelope))
                {
                    result.Add(receiver);
                }
            }

            return result;
        }

        private async Task<bool> HandleMessageAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                if (_autoNotify)
                {
                    await _sender.SendNotificationAsync(message.ToReceivedNotification(), cancellationToken);
                }

                await CallReceiversAsync(message, cancellationToken);

                if (_autoNotify)
                {
                    await _sender.SendNotificationAsync(message.ToConsumedNotification(), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Reason reason;
                if (ex is LimeException limeException)
                {
                    reason = limeException.Reason;
                }
                else
                {
                    reason = new Reason
                    {
                        Code = ReasonCodes.APPLICATION_ERROR,
                        Description = ex.Message
                    };
                }

                if (_autoNotify)
                {
                    await _sender.SendNotificationAsync(message.ToFailedNotification(reason), CancellationToken.None);
                }

                Trace.TraceError(ex.ToString());
            }

            return true;
        }

        private async Task<bool> HandleNotificationAsync(Notification notification, CancellationToken cancellationToken)
        {
            try
            {
                await CallReceiversAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }

            return true;
        }

        private async Task<bool> HandleCommandAsync(Command command, CancellationToken cancellationToken)
        {
            if (command.Status != CommandStatus.Pending) return true;

            try
            {
                await CallReceiversAsync(command, cancellationToken);
            }
            catch (Exception ex)
            {
                await _sender.SendCommandAsync(new Command
                {
                    Id = command.Id,
                    To = command.From,
                    Method = command.Method,
                    Status = CommandStatus.Failure,
                    Reason = ex.ToReason(),
                }, cancellationToken);
            }

            return true;
        }

        private async Task CallReceiversAsync<TEnvelope>(TEnvelope envelope, CancellationToken cancellationToken)
            where TEnvelope : Envelope, new()
        {
            var receivers = await GetReceiversAsync(envelope);

            // Gets the first non empty group, ordered by priority
            var receiverGroup = receivers
                .GroupBy(r => r.Priority)
                .OrderBy(r => r.Key)
                .First(r => r.Any());

            await Task.WhenAll(
                receiverGroup.Select(r => r.ReceiverFactory().ReceiveAsync(envelope, cancellationToken)));
        }
        
        private class ReceiverFactoryPredicate<T> where T : Envelope, new()
        {
            public ReceiverFactoryPredicate(Func<IEnvelopeReceiver<T>> receiverFactory, Func<T, Task<bool>> predicate, int priority)
            {
                ReceiverFactory = receiverFactory ?? throw new ArgumentNullException(nameof(receiverFactory));
                Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
                Priority = priority;
            }

            public Func<IEnvelopeReceiver<T>> ReceiverFactory { get; }

            public Func<T, Task<bool>> Predicate { get; }

            public int Priority { get; set; }
        }
    }
}
