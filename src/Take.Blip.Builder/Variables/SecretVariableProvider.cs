using System.Text;
using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Serialization;
using Serilog;
using Take.Blip.Client;
using Take.Blip.Builder.Actions.SendMessage;
using System.Linq;

namespace Take.Blip.Builder.Variables
{
    public class SecretVariableProvider : ResourceVariableProviderBase, IVariableProvider
    {
        private readonly string[] _actionsToBlock = { nameof(Actions.SendMessage), nameof(Actions.SetVariable), nameof(Actions.SetBucket) };

        public SecretVariableProvider(ISender sender, IDocumentSerializer documentSerializer, ILogger logger) : base(sender, documentSerializer, "secrets", logger, "postmaster@builder.msging.net") { }

        public override VariableSource Source => VariableSource.Secret;

        public override async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken, string stateActionType = null)
        {
            if (_actionsToBlock.Contains(stateActionType))
            {
                return null;
            }

            var variableValue = await base.GetVariableAsync(name, context, cancellationToken, stateActionType);

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
    }
}
