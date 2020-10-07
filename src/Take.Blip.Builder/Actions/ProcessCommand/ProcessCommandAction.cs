using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.ProcessCommand
{
    public class ProcessCommandAction : IAction
    {
        private readonly ISender _sender;
        private readonly IEnvelopeSerializer _envelopeSerializer;

        private const string RESOURCE_KEY = "resource";
        private const string RESOURCE_TYPE_KEY = "type";
        private const string SERIALIZABLE_TYPE = "json";

        public ProcessCommandAction(ISender sender, IEnvelopeSerializer envelopeSerializer)
        {
            _sender = sender;
            _envelopeSerializer = envelopeSerializer;
        }

        public string Type => nameof(ProcessCommand);

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings), $"The settings are required for '{nameof(ProcessCommandAction)}' action");

            string variable = null;

            if (settings.TryGetValue(nameof(variable), out var variableToken))
            {
                variable = variableToken.ToString().Trim('"');
            }

            var command = ConvertToCommand(settings);
            command.Id = EnvelopeId.NewId();

            var resultCommand = await _sender.ProcessCommandAsync(command, cancellationToken);

            if (string.IsNullOrWhiteSpace(variable)) return;

            var resultCommandJson = _envelopeSerializer.Serialize(resultCommand);
            await context.SetVariableAsync(variable, resultCommandJson, cancellationToken);
        }

        private Command ConvertToCommand(JObject settings)
        {
            if (settings.TryGetValue(RESOURCE_TYPE_KEY, out var type)
                && type.ToString().Contains(SERIALIZABLE_TYPE, StringComparison.OrdinalIgnoreCase)
                && settings.TryGetValue(RESOURCE_KEY, out var resource))
            {
                settings.Property(RESOURCE_KEY).Value = JObject.Parse(resource.ToString());
            }
            return settings.ToObject<Command>(LimeSerializerContainer.Serializer);
        }
    }
}