using System.Threading.Tasks;
using System.Threading;
using Lime.Protocol.Serialization;
using Serilog;
using Take.Blip.Client;
using Lime.Protocol;
using System;
using Take.Blip.Builder.Utils;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Variables
{
    /// <summary>
    /// Provides variables from Blip functions.
    /// </summary>
    public class BlipFunctionVariableProvider : ResourceVariableProviderBase, IVariableProvider
    {
        private ISender _sender;
        private string APPLICATION_NAME = "functions";
        private static readonly Node BuilderAddress = Node.Parse($"postmaster@builder.msging.net");
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlipFunctionVariableProvider"/> class.
        /// </summary>
        /// <param name="sender">The sender to use for sending commands.</param>
        /// <param name="documentSerializer">The document serializer to use.</param>
        /// <param name="logger">The logger to use for logging.</param>
        public BlipFunctionVariableProvider(ISender sender, IDocumentSerializer documentSerializer, ILogger logger)
            : base(sender, documentSerializer, "functions", logger, "postmaster@builder.msging.net") {
            _sender = sender;
            _logger = logger;
        }

        
        /// <summary>
        /// Gets the source of the variable.
        /// </summary>
        public override VariableSource Source => VariableSource.BlipFunction;

        /// <summary>
        /// Gets the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="context">The context in which the variable is being requested.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The value of the variable.</returns>
        public override async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
            var getFunctionCommand = GenerateFunctionCommand(name);
            try
            {
                var resourceCommandResult = await _sender.ProcessCommandAsync(
                getFunctionCommand,
                cancellationToken);

                if (resourceCommandResult.Status != CommandStatus.Success)
                {
                    _logger.Warning("Variable {VariableName} from {ResourceName} not found", name, APPLICATION_NAME);
                    return null;
                }

                var function = ((DocumentCollection)resourceCommandResult.Resource).Items[0].ToObject<Function>();

                return !string.IsNullOrEmpty(function.FunctionContent) ? function.FunctionContent : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private Command GenerateFunctionCommand(string name)
        {
            var command = new Command()
            {
                Uri = new LimeUri($"/{APPLICATION_NAME}?functionName={name}"),
                To = BuilderAddress,
                Method = CommandMethod.Get
            };

            return command;
        }
    }
}
