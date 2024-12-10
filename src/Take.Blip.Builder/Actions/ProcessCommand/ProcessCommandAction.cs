using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.ProcessCommand
{
    public class ProcessCommandAction : IAction
    {
        private readonly ISender _sender;
        private readonly IEnvelopeSerializer _envelopeSerializer;

        private const string SERIALIZABLE_PATTERN = @".+[/|\+]json$";

        public ProcessCommandAction(ISender sender, IEnvelopeSerializer envelopeSerializer)
        {
            _sender = sender;
            _envelopeSerializer = envelopeSerializer;
        }

        public string Type => nameof(ProcessCommand);

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings), $"The settings are required for '{nameof(ProcessCommandAction)}' action");

            string variable = null;

            if (settings.TryGetValue(nameof(variable), out var variableToken))
            {
                variable = variableToken.ToString().Trim('"');
            }

            var command = ConvertToCommand(settings);
            command.Id = EnvelopeId.NewId();

            var resultCommand = await _sender.ProcessCommandAsync(command, cancellationToken);

            if (string.IsNullOrWhiteSpace(variable))
                return;

            var resultCommandJson = _envelopeSerializer.Serialize(resultCommand);
            await context.SetVariableAsync(variable, resultCommandJson, cancellationToken);
        }

        private Command ConvertToCommand(JObject settings)
        {

            if (settings.TryGetValue(Command.TYPE_KEY, out var type)
                && Regex.IsMatch(type.ToString(), SERIALIZABLE_PATTERN, default, Constants.REGEX_TIMEOUT)
                && settings.TryGetValue(Command.RESOURCE_KEY, out var resource))
            {
                settings.Property(Command.RESOURCE_KEY).Value = JObject.Parse(resource.ToString());
            }

            var command = settings.ToObject<Command>(LimeSerializerContainer.Serializer);
            InsertMetadataToRouteCommandProperly(command);

            return command;
        }

        private void InsertMetadataToRouteCommandProperly(Command command)
        {
            if (command.Metadata is null)
                command.Metadata = new Dictionary<string, string>();

            if (command.Metadata.ContainsKey(Client.Constants.ENVELOPE_MUST_BE_ROUTED_TO_SERVER_KEY))
                command.Metadata[Client.Constants.ENVELOPE_MUST_BE_ROUTED_TO_SERVER_KEY] = "true";
            else
                command.Metadata.Add(Client.Constants.ENVELOPE_MUST_BE_ROUTED_TO_SERVER_KEY, "true");
        }
    }
}
