using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Serilog;
using Take.Blip.Client;

namespace Take.Blip.Builder.Variables
{
    /// <summary>
    /// Variable replacer that only will allow variable substitution on Process Http Action, since that with other actions, you can send the secret to other variables and see the value explicitly
    /// </summary>
    [VariableProviderRestriction(AllowedActions =  new [] { nameof(Actions.ProcessHttp) })]
    public class SecretVariableProvider : ResourceVariableProviderBase, IVariableProvider
    {
        private ISender _sender;
        private static string APPLICATION_NAME = "secrets";
        private static readonly string BUILDER_ADDRESS = "postmaster@builder.msging.net";
        private readonly ILogger _logger;
        public SecretVariableProvider(ISender sender, IDocumentSerializer documentSerializer, ILogger logger) : base(sender, documentSerializer, APPLICATION_NAME, logger, BUILDER_ADDRESS) 
        {
            _sender = sender;
            _logger = logger;
        }

        public override VariableSource Source => VariableSource.Secret;

        public override async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {

            if(context.Flow.BuilderConfiguration.UseTunnelOwnerContext == true)
            {
                var result = await _sender.ProcessCommandAsync(GenerateCommand(name, context.Input.Message.To), cancellationToken);

                if(result.Status != CommandStatus.Success)
                {
                    _logger.Warning("Variable {VariableName} from {ResourceName} not found", name, APPLICATION_NAME);
                    return null;
                }

                return GetSecretValue(result.Resource.ToString());
            }


            var variableValue = await base.GetVariableAsync(name, context, cancellationToken);

            return GetSecretValue(variableValue);
        }

        private static string GetSecretValue(string value)
        {
            if (value is null)
            {
                return value;
            }

            var plainTextBytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(plainTextBytes);
        }

        private Command GenerateCommand(string name, Node from)
        {
            var command = new Command()
            {
                Uri = new LimeUri($"/{APPLICATION_NAME}/{name}"),
                To = BUILDER_ADDRESS,
                Method = CommandMethod.Get,
                From = from
            };

            return command;
        }
    }
}
