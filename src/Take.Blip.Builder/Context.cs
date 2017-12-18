using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder
{
    public class Context : IContext
    {
        public const string INTERVAL_VARIABLE_NAME_PREFIX = "$";

        private readonly IContextExtension _contextExtension;        

        public Context(IContextExtension contextExtension, string flowId, Identity user)
        {
            _contextExtension = contextExtension;
            User = user ?? throw new ArgumentNullException(nameof(user));
            FlowId = flowId;
        }

        public string FlowId { get; set; }

        public Identity User { get; }

        public Task SetVariableAsync(string name, string value, CancellationToken cancellationToken)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return _contextExtension.SetTextVariableAsync(User, name, value, cancellationToken);
        }

        public async Task<string> GetVariableAsync(string name, CancellationToken cancellationToken)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var variableAndProperty = name.Split('@');
            var variableName = variableAndProperty[0];
            var propertyName = variableAndProperty.Length > 1 ? variableAndProperty[1] : null;

            try
            {
                var variableValue = await _contextExtension.GetTextVariableAsync(User, variableName, cancellationToken);
                if (string.IsNullOrWhiteSpace(propertyName)) return variableValue;

                // If there's a propertyName, attempts to parse the value as JSON and retrieve the value from it.
                var propertyNames = propertyName.Split('.');
                JToken json = JObject.Parse(variableValue);
                foreach (var s in propertyNames)
                {
                    json = json[s];
                    if (json == null) return null;
                }

                return json.ToString(Formatting.None).Trim('"');
            }
            catch (Exception ex) when (
                (ex is LimeException limeException && limeException.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
                || ex is JsonException)
            {
                return null;
            }
        }
    }
}