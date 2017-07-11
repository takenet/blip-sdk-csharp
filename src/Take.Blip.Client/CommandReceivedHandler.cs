using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client
{
    internal sealed class CommandReceivedHandler : EnvelopeReceivedHandler<Command>
    {
        private readonly ISender _sender;

        public CommandReceivedHandler(ISender sender, EnvelopeReceiverManager envelopeManager)
            : base(envelopeManager)
        {
            _sender = sender;
        }

        protected override async Task CallReceiversAsync(Command command, CancellationToken cancellationToken)
        {
            if (command.Status != CommandStatus.Pending) return;

            try
            {
                await base.CallReceiversAsync(command, cancellationToken);
            }
            catch (Exception ex)
            {
                await _sender.SendCommandAsync(new Command
                {
                    Id = command.Id,
                    To = command.From,
                    Method = command.Method,
                    Status = CommandStatus.Failure,
                    Reason = ex.ToReason(),
                }, cancellationToken);
            }
        }
    }
}