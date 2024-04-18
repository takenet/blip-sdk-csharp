using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly string[] _actionsToAllow = { nameof(Actions.ProcessHttp) };

        public SecretVariableProvider(ISender sender, IDocumentSerializer documentSerializer, ILogger logger) : base(sender, documentSerializer, "secrets", logger, "postmaster@builder.msging.net") { }

        public override VariableSource Source => VariableSource.Secret;

        public override async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
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
    }
}
