using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lime.Protocol;
using Lime.Protocol.Listeners;
using Lime.Protocol.Network;
using Lime.Protocol.Util;
using Serilog;
using Serilog.Context;
using Take.Blip.Client.Receivers;

namespace Take.Blip.Client
{
    public class BlipChannelListener : IBlipChannelListener, IDisposable, IMessageReceiver, INotificationReceiver, ICommandReceiver
    {
        private readonly ISender _sender;
        private readonly bool _autoNotify;
        private readonly ILogger _logger;

        private readonly IChannelListener _channelListener;
        private readonly IList<ReceiverFactoryPredicate<Message>> _messageReceivers;
        private readonly IList<ReceiverFactoryPredicate<Notification>> _notificationReceivers;
        private readonly IList<ReceiverFactoryPredicate<Command>> _commandReceivers;

        private readonly ActionBlock<Message> _messageActionBlock;
        private readonly ActionBlock<Notification> _notificationActionBlock;
        private readonly ActionBlock<Command> _commandActionBlock;

        private CancellationTokenSource _cts;
        private readonly object _syncRoot = new object();

        public BlipChannelListener(ISender sender, bool autoNotify, ILogger logger = null)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _autoNotify = autoNotify;
            _logger = logger ?? LoggerProvider.Logger;

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
                m => ReceiveAsync(m, _cts.Token),
                dataflowBlockOptions);
            _notificationActionBlock = new ActionBlock<Notification>(
                n => ReceiveAsync(n, _cts.Token),
                dataflowBlockOptions);
            _commandActionBlock = new ActionBlock<Command>(
                c => ReceiveAsync(c, _cts.Token),
                dataflowBlockOptions);
            _channelListener = new DataflowChannelListener(
                _messageActionBlock,
                _notificationActionBlock,
                _commandActionBlock);
        }

        public Task<Message> MessageListenerTask => _channelListener.MessageListenerTask;

        public Task<Notification> NotificationListenerTask => _channelListener.NotificationListenerTask;

        public Task<Command> CommandListenerTask => _channelListener.CommandListenerTask;

        public void AddMessageReceiver(IMessageReceiver messageReceiver, Func<Message, Task<bool>> messageFilter = null, int priority = 0)
        {
            if (messageReceiver == null) throw new ArgumentNullException(nameof(messageReceiver));
            AddEnvelopeReceiver(_messageReceivers, () => messageReceiver, messageFilter, priority);
        }

        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Func<Notification, Task<bool>> notificationFilter = null, int priority = 0)
        {
            if (notificationReceiver == null) throw new ArgumentNullException(nameof(notificationReceiver));
            AddEnvelopeReceiver(_notificationReceivers, () => notificationReceiver, notificationFilter, priority);
        }

        public void AddCommandReceiver(ICommandReceiver commandReceiver, Func<Command, Task<bool>> commandFilter = null, int priority = 0)
        {
            if (commandReceiver == null) throw new ArgumentNullException(nameof(commandReceiver));
            AddEnvelopeReceiver(_commandReceivers, () => commandReceiver, commandFilter, priority);
        }

        public void Start(IEstablishedReceiverChannel channel)
        {
            lock (_syncRoot)
            {
                if (_cts != null)
                {
                    throw new InvalidOperationException("The listener is already started");
                }

                _cts = new CancellationTokenSource();
                _channelListener.Start(channel);
            }
        }

        public void Stop()
        {
            lock (_syncRoot)
            {
                if (_cts == null)
                {
                    throw new InvalidOperationException("The listener is not started");
                }

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
            if (receiverFactory == null)
            {
                throw new ArgumentNullException(nameof(receiverFactory));
            }

            if (predicate == null)
            {
                predicate = envelope => TaskUtil.TrueCompletedTask;
            }

            var predicateReceiverFactory = new ReceiverFactoryPredicate<T>(receiverFactory, predicate, priority);
            envelopeReceivers.Add(predicateReceiverFactory);
        }

        private async Task<IEnumerable<ReceiverFactoryPredicate<TEnvelope>>> GetReceiversAsync<TEnvelope>(TEnvelope envelope)
            where TEnvelope : Envelope, new()
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

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

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var shouldNotify = _autoNotify && !string.IsNullOrWhiteSpace(message?.Id);

            try
            {
                if (shouldNotify)
                {
                    await _sender.SendNotificationAsync(message.ToReceivedNotification(), cancellationToken);
                }

                using (EnvelopeReceiverContext<Message>.Create(message))
                {
                    await CallReceiversAsync(message, cancellationToken);
                }

                if (shouldNotify)
                {
                    await _sender.SendNotificationAsync(message.ToConsumedNotification(), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                using (LogContext.PushProperty(nameof(Message.Type), message.Type))
                {
                    LogException(message, ex);
                }

                // Always notify failures
                if (!string.IsNullOrWhiteSpace(message?.Id))
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

                    await _sender.SendNotificationAsync(message.ToFailedNotification(reason), CancellationToken.None);
                }
            }
        }

        public async Task ReceiveAsync(Notification notification, CancellationToken cancellationToken)
        {
            try
            {
                using (EnvelopeReceiverContext<Notification>.Create(notification))
                {
                    await CallReceiversAsync(notification, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                LogException(notification, ex);
            }
        }

        public async Task ReceiveAsync(Command command, CancellationToken cancellationToken)
        {
            if (command.Status != CommandStatus.Pending)
            {
                return;
            }

            try
            {
                using (EnvelopeReceiverContext<Command>.Create(command))
                {
                    await CallReceiversAsync(command, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                LogException(command, ex);

                await _sender.SendCommandAsync(new Command
                {
                    Id = command.Id,
                    To = command.From,
                    Method = command.Method,
                    Status = CommandStatus.Failure,
                    Reason = ex.ToReason(),
                },
                cancellationToken);
            }
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
                receiverGroup.Select(r =>
                {
                    var receiver = r.ReceiverFactory();
                    if (receiver == null)
                    {
                        throw new ApplicationException("A receiver factory produced a null instance"); 
                    }
                
                    return receiver.ReceiveAsync(envelope, cancellationToken);
                }));
        }

        private void LogException<T>(T envelope, Exception ex) where T : Envelope
        {
            using (LogContext.PushProperty(nameof(Envelope), typeof(T).Name))
            using (LogContext.PushProperty(nameof(Envelope.Id), envelope.Id))
            using (LogContext.PushProperty(nameof(Envelope.From), envelope.From))
            using (LogContext.PushProperty(nameof(Envelope.To), envelope.To))
            {
                _logger.Error(ex, $"Error processing the received envelope: {ex.Message}");
            }
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

            public int Priority { get; }
        }
    }
}