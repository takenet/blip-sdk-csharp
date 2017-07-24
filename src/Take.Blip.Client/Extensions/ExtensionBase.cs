using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;

namespace Take.Blip.Client.Extensions
{
    public abstract class ExtensionBase
    {
        protected readonly ISender Sender;

        protected ExtensionBase(ISender sender)
        {
            Sender = sender;
        }

        protected async Task ProcessCommandAsync(Command requestCommand, CancellationToken cancellationToken)
        {
            var responseCommand = await Sender
                .ProcessCommandAsync(requestCommand, cancellationToken)
                .ConfigureAwait(false);

            EnsureSuccess(responseCommand);
        }

        protected async Task<T> ProcessCommandAsync<T>(Command requestCommand, CancellationToken cancellationToken) where T : Document
        {
            var responseCommand = await Sender
                .ProcessCommandAsync(requestCommand, cancellationToken)
                .ConfigureAwait(false);

            EnsureSuccess(responseCommand);

            return responseCommand.Resource as T;
        }

        protected Command CreateSetCommandRequest<T>(T resource, string uriPath, Node to = null) where T : Document =>
            new Command()
            {
                To = to,
                Method = CommandMethod.Set,
                Uri = new LimeUri(uriPath),
                Resource = resource
            };

        protected Command CreateGetCommandRequest(string uriPath, Node to = null) =>
            new Command()
            {
                To = to,
                Method = CommandMethod.Get,
                Uri = new LimeUri(uriPath)
            };

        protected Command CreateDeleteCommandRequest(string uriPath, Node to = null) =>
            new Command()
            {
                To = to,
                Method = CommandMethod.Delete,
                Uri = new LimeUri(uriPath)
            };

        protected virtual void EnsureSuccess(Command responseCommand)
        {
            if (responseCommand.Status != CommandStatus.Success)
            {
                throw new LimeException(
                    responseCommand.Reason ??
                    new Reason() { Code = ReasonCodes.COMMAND_PROCESSING_ERROR, Description = "An error occurred" });
            }
        }
    }
}