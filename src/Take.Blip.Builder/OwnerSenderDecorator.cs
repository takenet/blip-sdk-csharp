using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;
using Take.Blip.Client.Activation;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a decorator of <see cref="ISender"/> that checks <see cref="OwnerContext"/> for handling commands before sending.
    /// </summary>
    public sealed class OwnerSenderDecorator : ISender
    {
        private readonly ISender _sender;
        private readonly Identity _applicationIdentity;

        public OwnerSenderDecorator(ISender sender, Application application)
        {
            _sender = sender;
            _applicationIdentity = application.Identity;
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

        private Command Intercept(Command command)
        {
            if (command.From == null)
            {
                var owner = OwnerContext.Owner;
                if (owner != null &&
                    owner != _applicationIdentity)
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