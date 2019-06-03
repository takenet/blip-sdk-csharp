using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a decorator of <see cref="ISender"/> that checks <see cref="OwnerContext"/> for handling commands before sending.
    /// </summary>
    public sealed class OwnerSenderDecorator : ISender
    {
        private readonly ISender _sender;

        public OwnerSenderDecorator(ISender sender)
        {
            _sender = sender;
        }

        public Task SendMessageAsync(Message message, CancellationToken cancellationToken)
        {
            return _sender.SendMessageAsync(message, cancellationToken);
        }

        public Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken)
        {
            return _sender.SendNotificationAsync(notification, cancellationToken);
        }

        public Task SendCommandAsync(Command command, CancellationToken cancellationToken)
        {
            return _sender.SendCommandAsync(Intercept(command), cancellationToken);
        }

        public Task<Command> ProcessCommandAsync(Command requestCommand, CancellationToken cancellationToken)
        {
            return _sender.ProcessCommandAsync(Intercept(requestCommand), cancellationToken);
        }

        private static Command Intercept(Command command)
        {
            if (command.From == null)
            {
                var owner = OwnerContext.Owner;
                if (owner != null)
                {
                    var ownerCommand = command.ShallowCopy();
                    ownerCommand.From = owner.ToNode();
                    return ownerCommand;
                }
            }

            return command;
        }
    }
}