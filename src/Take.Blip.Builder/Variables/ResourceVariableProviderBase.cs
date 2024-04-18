using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Serilog;
using Take.Blip.Client;

namespace Take.Blip.Builder.Variables
{
    public abstract class ResourceVariableProviderBase : IVariableProvider
    {
        private readonly ISender _sender;
        private readonly IDocumentSerializer _documentSerializer;
        private readonly string _resourceName;
        private readonly ILogger _logger;
        private readonly string _commandDestination;

        public abstract VariableSource Source { get; }

        protected ResourceVariableProviderBase(ISender sender, IDocumentSerializer documentSerializer, string resourceName, ILogger logger, string commandDestination = "")
        {
            _sender = sender;
            _documentSerializer = documentSerializer;
            _logger = logger;
            _resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
            _commandDestination = commandDestination;
        }


        public virtual async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken, string stateActionType = null)
        {
            try
            {
                var resourceCommandResult = await ExecuteGetResourceCommandAsync(name, cancellationToken);

                if (resourceCommandResult.Status != CommandStatus.Success)
                {
                    _logger.Warning("Variable {VariableName} from {ResourceName} not found", name, _resourceName);
                    return null;
                }

                if (!resourceCommandResult.Resource.GetMediaType().IsJson)
                {
                    return resourceCommandResult.Resource.ToString();
                }

                return _documentSerializer.Serialize(resourceCommandResult.Resource);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                _logger.Warning(ex, "An exception occurred while obtaining variable {VariableName} from {ResourceName}", name, _resourceName);
                return null;
            }
        }

        private async Task<Command> ExecuteGetResourceCommandAsync(string name, CancellationToken cancellationToken)
        {
            // We are sending the command directly here because the Extension requires us to know the type.
            var getResourceCommand = GenerateResourceCommand(name);

            var resourceCommandResult = await _sender.ProcessCommandAsync(
                getResourceCommand,
                cancellationToken);

            return resourceCommandResult;
        }

        private Command GenerateResourceCommand(string name)
        {
            var command = new Command()
            {
                Uri = new LimeUri($"/{_resourceName}/{Uri.EscapeDataString(name)}"),
                Method = CommandMethod.Get
            };

            if (!string.IsNullOrEmpty(_commandDestination))
            {
                command.To = _commandDestination;
            }
            return command;
        }
    }
}
