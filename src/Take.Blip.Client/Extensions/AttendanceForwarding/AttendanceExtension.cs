using Lime.Protocol;
using System.Threading;
using System.Threading.Tasks;
using Takenet.Iris.Messaging.Contents;

namespace Take.Blip.Client.Extensions.AttendanceForwarding
{
    public class AttendanceExtension : IAttendanceExtension
    {
        private readonly ISender _sender;

        public AttendanceExtension(ISender sender)
        {
            _sender = sender;
        }

        public Task ForwardMessageToAttendantAsync(Message originalMessage, string attendanceIdentity, CancellationToken cancellationToken)
        {
            var forwardDestinationNode = GetAttendantDestinationNode(attendanceIdentity);
            var newMessage = new Message
            {
                Id = EnvelopeId.NewId(),
                To = forwardDestinationNode,
                Content = new Attendance
                {
                    Attendant = forwardDestinationNode,
                    Customer = originalMessage.From,
                    Content = new DocumentContainer { Value = originalMessage.Content }
                }
            };

            return _sender.SendMessageAsync(newMessage, cancellationToken);
        }

        public Task ForwardAttendantReplyAsync(Message replyMessage, CancellationToken cancellationToken)
        {
            var forwardContent = replyMessage.Content as Attendance;
            var newMessage = new Message
            {
                Id = replyMessage.Id,
                To = forwardContent.Customer,
                Content = forwardContent.Content.Value
            };

            return _sender.SendMessageAsync(newMessage, cancellationToken);
        }

        public bool FromAttendant(Message message, string attendantIdentity)
        {
            return message.Content?.GetMediaType() == Attendance.MediaType &&
                   message.From.ToIdentity().Equals(GetAttendantDestinationNode(attendantIdentity).ToIdentity());

        }

        private static Node GetAttendantDestinationNode(string forwardDestination)
        {
            var node = Node.Parse(forwardDestination);
            node.Domain = node.Domain ?? "0mn.io";
            return node;
        }

    }
}
