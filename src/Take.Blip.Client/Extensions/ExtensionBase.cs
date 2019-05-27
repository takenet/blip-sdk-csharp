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

        protected Command CreateSetCommandRequest<T>(
            T resource,
            string uriPath,
            Node to = null,
            string id = null,
            Node @from = null) where T : Document =>
            new Command(id ?? EnvelopeId.NewId())
            {
                From = from,
                To = to,
                Method = CommandMethod.Set,
                Uri = new LimeUri(uriPath),
                Resource = resource
            };

        protected Command CreateMergeCommandRequest<T>(
            T resource,
            string uriPath,
            Node to = null,
            string id = null,
            Node from = null) where T : Document =>
            new Command(id ?? EnvelopeId.NewId())
            {
                From = from,
                To = to,
                Method = CommandMethod.Merge,
                Uri = new LimeUri(uriPath),
                Resource = resource
            };

        protected Command CreateGetCommandRequest(string uriPath, Node to = null, string id = null, Node from = null) =>
            new Command(id ?? EnvelopeId.NewId())
            {
                From = from,
                To = to,
                Method = CommandMethod.Get,
                Uri = new LimeUri(uriPath)
            };

        protected Command CreateDeleteCommandRequest(
            string uriPath, 
            Node to = null, 
            string id = null, 
            Node from = null) =>
            new Command(id ?? EnvelopeId.NewId())
            {
                From = from,
                To = to,
                Method = CommandMethod.Delete,
                Uri = new LimeUri(uriPath)
            };

        protected Command CreateObserveCommandRequest<T>(
            string uriPath,
            T resource = default,
            Node to = null,
            string id = null,
            Node from = null) where T : Document =>
            new Command(id)
            {
                From = from,
                To = to,
                Method = CommandMethod.Observe,
                Uri = new LimeUri(uriPath),
                Resource = resource
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