using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.AttendanceForwarding
{
    /// <summary>
    /// Provide human attendance forwarding, using Blip App as 'operator'
    /// </summary>
    [Obsolete("IAttendanceExtension is deprecated, please use IDeskExtension instead")]
    public interface IAttendanceExtension
    {
        /// <summary>
        /// Forward a message to a BLIP App account.
        /// (when to forward (what conditions a user must fulfill) 
        /// is set by application logic).
        /// </summary>
        /// <param name="originalMessage">The message to be forwarded to BLIP</param>
        /// <param name="attendanceIdentity">The BLIP App number which will received the <paramref name="originalMessage"/></param>
        Task ForwardMessageToAttendantAsync(Message originalMessage, string attendanceIdentity, CancellationToken cancellationToken);

        /// <summary>
        /// Forward a reply from a BLIP App account to original sender
        /// </summary>
        /// <param name="replyMessage">The reply message from BLIP App</param>
        Task ForwardAttendantReplyAsync(Message replyMessage, CancellationToken cancellationToken);

        /// <summary>
        /// Check if a message is a reply from a BLIP App account 
        /// </summary>
        /// <param name="attendantIdentity">The BLIP App number which receives messages</param>
        bool FromAttendant(Message message, string attendantIdentity);
    }
}