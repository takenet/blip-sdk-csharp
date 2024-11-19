using System.Threading.Tasks;
using System.Threading;
using Lime.Protocol.Serialization;
using Serilog;
using Take.Blip.Client;

namespace Take.Blip.Builder.Variables
{
    /// <summary>
    /// Provides variables from Blip functions.
    /// </summary>
    public class BlipFunctionVariableProvider : ResourceVariableProviderBase, IVariableProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlipFunctionVariableProvider"/> class.
        /// </summary>
        /// <param name="sender">The sender to use for sending commands.</param>
        /// <param name="documentSerializer">The document serializer to use.</param>
        /// <param name="logger">The logger to use for logging.</param>
        public BlipFunctionVariableProvider(ISender sender, IDocumentSerializer documentSerializer, ILogger logger)
            : base(sender, documentSerializer, "blipFunction", logger, "postmaster@builder.msging.net") { }

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
            var variableValue = await base.GetVariableAsync(name, context, cancellationToken);

            return variableValue;
        }
    }
}
