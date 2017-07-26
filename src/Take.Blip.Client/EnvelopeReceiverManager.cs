using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client.Receivers;

namespace Take.Blip.Client
{
    internal class EnvelopeReceiverManager
    {
        private readonly IList<ReceiverFactoryPredicate<Message>> _messageReceivers;
        private readonly IList<ReceiverFactoryPredicate<Notification>> _notificationReceivers;
        private readonly IList<ReceiverFactoryPredicate<Command>> _commandReceivers;

        internal EnvelopeReceiverManager()
        {
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
        }

        /// <summary>
        /// Add a message receiver listener to handle received messages.
        /// </summary>
        /// <param name="receiverFactory">A function used to build the notification listener</param>
        /// <param name="predicate">The message predicate used as a filter of messages received by listener.</param>
        /// <param name="priority"></param>
        public void AddMessageReceiver(Func<IMessageReceiver> receiverFactory, Func<Message, Task<bool>> predicate, int priority) =>
            AddEnvelopeReceiver(_messageReceivers, receiverFactory, predicate, priority);


        internal void AddCommandReceiver(Func<ICommandReceiver> receiverFactory, Func<Command, Task<bool>> predicate, int priority) =>
            AddEnvelopeReceiver(_commandReceivers, receiverFactory, predicate, priority);

        /// <summary>
        /// Add a notification receiver listener to handle received notifications.
        /// </summary>
        /// <param name="receiverFactory">A function used to build the notification listener</param>
        /// <param name="predicate">The notification predicate used as a filter of notifications received by listener.</param>
        /// <param name="priority"></param>
        public void AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Func<Notification, Task<bool>> predicate, int priority) =>
            AddEnvelopeReceiver(_notificationReceivers, receiverFactory, predicate, priority);

        public bool HasRegisteredReceivers => _messageReceivers.Any() || _notificationReceivers.Any();

        public async Task<IEnumerable<ReceiverFactoryPredicate<TEnvelope>>> GetReceiversAsync<TEnvelope>(TEnvelope envelope)
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

        private void AddEnvelopeReceiver<T>(
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

        internal class ReceiverFactoryPredicate<T> where T : Envelope, new()
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
