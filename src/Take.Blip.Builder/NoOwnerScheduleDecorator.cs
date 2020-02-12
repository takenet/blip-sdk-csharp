using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client.Extensions.Scheduler;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a decorator of <see cref="ISchedulerExtension"/> that remove <see cref="OwnerContext"/> for handling commands before sending.
    /// </summary>
    public class NoOwnerScheduleDecorator : ISchedulerExtension
    {
        private readonly ISchedulerExtension _schedulerExtension;

        /// <summary>
        /// OwnerContext to dispose and recreate
        /// </summary>
        public IDisposable OwnerDisposable { get; private set; }

        /// <summary>
        /// Identity of owner to recreate owner context
        /// </summary>
        public Identity OwnerIdentity { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="schedulerExtension"></param>
        /// <param name="ownerContext"></param>
        /// <param name="ownerIdentity"></param>
        public NoOwnerScheduleDecorator(ISchedulerExtension schedulerExtension, IDisposable ownerContext, Identity ownerIdentity)
        {
            _schedulerExtension = schedulerExtension;
            OwnerDisposable = ownerContext;
            OwnerIdentity = ownerIdentity;
        }

        /// <summary>
        /// Cancel a message scheduled without owner
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task CancelScheduledMessageAsync(string messageId, CancellationToken cancellationToken = default)
        {
            OwnerDisposable?.Dispose();
            await _schedulerExtension.CancelScheduledMessageAsync(messageId, cancellationToken);
            OwnerDisposable = OwnerContext.Create(OwnerIdentity);
        }

        /// <summary>
        /// Get a schedule message by messageid without owner
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Schedule> GetScheduledMessageAsync(string messageId, CancellationToken cancellationToken = default)
        {
            OwnerDisposable?.Dispose();
            var schedule = await _schedulerExtension.GetScheduledMessageAsync(messageId, cancellationToken);
            OwnerDisposable = OwnerContext.Create(OwnerIdentity);
            return schedule;
        }

        /// <summary>
        /// schedule a message in param without owner
        /// </summary>
        /// <param name="message"><see cref="Message"/> that will be sent</param>
        /// <param name="when">When the message will be sent</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ScheduleMessageAsync(Message message, DateTimeOffset when, CancellationToken cancellationToken = default)
        {
            OwnerDisposable?.Dispose();
            await _schedulerExtension.ScheduleMessageAsync(message, when, cancellationToken);
            OwnerDisposable = OwnerContext.Create(OwnerIdentity);
        }
    }
}
