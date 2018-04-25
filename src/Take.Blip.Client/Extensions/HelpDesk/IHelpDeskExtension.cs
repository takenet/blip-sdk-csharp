using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.HelpDesk
{
    /// <summary>
    /// Provide human attendance forwarding, using Blip HelpDesks module
    /// </summary>
    public interface IHelpDeskExtension
    {
        /// <summary>
        /// Forward a message to active HelpDesk application.
        /// </summary>
        /// <param name="message">The message to be forwarded to BLIP HelpDesks</param>
        Task ForwardMessageToAttendantAsync(Message message, CancellationToken cancellationToken);

        /// <summary>
        /// Check if a message is a reply from a BLIP HelpDesks application 
        /// </summary>
        /// <param name="message">The Message that must be analyzed</param>
        bool FromAttendant(Message message);
    }
}