using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Client.Extensions.Scheduler
{
    public class SchedulerExtension : ExtensionBase, ISchedulerExtension
    {
        private const string SCHEDULE_URI = "/schedules";
        private static readonly Node SchedulerAddress = Node.Parse($"postmaster@scheduler.{Constants.DEFAULT_DOMAIN}");

        public SchedulerExtension(ISender sender) 
            : base(sender)
        {
        }

        public Task ScheduleMessageAsync(Message message, DateTimeOffset when,
            Node from = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            return ProcessCommandAsync(
                CreateSetCommandRequest(
                    new Schedule
                    {
                        Message = message,
                        When = when
                    },
                    SCHEDULE_URI,
                    SchedulerAddress, from: from),
                cancellationToken);
        }

        public Task<Schedule> GetScheduledMessageAsync(string messageId,
            Node from = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (messageId == null) throw new ArgumentNullException(nameof(messageId));

            var scheduledMessage = $"{SCHEDULE_URI}/{messageId}";

            return ProcessCommandAsync<Schedule>(
                CreateGetCommandRequest(scheduledMessage, SchedulerAddress, from: from),
                cancellationToken);
        }

        public Task CancelScheduledMessageAsync(string messageId,
            Node from = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (messageId == null) throw new ArgumentNullException(nameof(messageId));

            var scheduledMessage = $"{SCHEDULE_URI}/{messageId}";

            return ProcessCommandAsync(
                CreateDeleteCommandRequest(scheduledMessage, SchedulerAddress, from: from),
                cancellationToken);
        }
    }
}