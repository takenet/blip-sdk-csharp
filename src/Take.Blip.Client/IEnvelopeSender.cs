using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;

namespace Take.Blip.Client
{
    /// <summary>
    /// Defines a service for sending envelopes throught the active connection.
    /// </summary>
    public interface IEnvelopeSender
    {
        /// <summary>
        /// Send a command through the available connection and awaits for its response.
        /// </summary>
        /// <param name="command">Command to be sent</param>
        /// <param name="cancellationToken">A cancellation token to allow the task to be canceled</param>
        /// <returns>A task representing the sending operation. When completed, it will contain the command response</returns>
        Task<Command> ProcessCommandAsync(Command command, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Send a command response through the available connection.
        /// </summary>
        /// <param name="command">Command to be sent</param>
        /// <param name="cancellationToken">A cancellation token to allow the task to be canceled</param>
        /// <returns>A task representing the sending operation.</returns>
        Task SendCommandResponseAsync(Command command, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Send a message through the available connection.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="cancellationToken">A cancellation token to allow the task to be canceled</param>
        /// <returns>A task representing the sending operation</returns>
        Task SendMessageAsync(Message message, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Send a notification through the available connection.
        /// </summary>
        /// <param name="notification">Notification to be sent</param>
        /// <param name="cancellationToken">A cancellation token to allow the task to be canceled</param>
        /// <returns>A task representing the sending operation</returns>
        Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Extension methods to simplify the usage of the <see cref="MessagingHubSender"/>
    /// </summary>
    public static class MessagingHubSenderExtensions
    {
        /// <summary>
        /// Send a message through the Messaging Hub
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="to">The destination of the message</param>
        /// <param name="cancellationToken">A cancellation token to allow the task to be canceled</param>
        public static Task SendMessageAsync(this IEnvelopeSender sender, string content, string to, CancellationToken cancellationToken = default(CancellationToken))
            => sender.SendMessageAsync(content, Node.Parse(to), cancellationToken);

        /// <summary>
        /// Send a message through the Messaging Hub
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="to">The destination of the message</param>
        /// <param name="cancellationToken">A cancellation token to allow the task to be canceled</param>
        public static Task SendMessageAsync(this IEnvelopeSender sender, string content, Node to, CancellationToken cancellationToken = default(CancellationToken))
            => sender.SendMessageAsync(CreatePlainTextContent(content) as Document, to, cancellationToken);

        /// <summary>
        /// Send a message through the Messaging Hub
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="to">The destination of the message</param>
        /// <param name="cancellationToken">A cancellation token to allow the task to be canceled</param>
        public static Task SendMessageAsync(this IEnvelopeSender sender, Document content, string to, CancellationToken cancellationToken = default(CancellationToken))
        => sender.SendMessageAsync(content, Node.Parse(to), cancellationToken);

        /// <summary>
        /// Send a message through the Messaging Hub
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="to">The destination of the message</param>
        /// <param name="cancellationToken">A cancellation token to allow the task to be canceled</param>
        public static Task SendMessageAsync(this IEnvelopeSender sender, Document content, Node to, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                To = to,
                Content = content
            };
            return sender.SendMessageAsync(message, cancellationToken);
        }

        private static PlainText CreatePlainTextContent(string content) => new PlainText { Text = content };
    }
}
