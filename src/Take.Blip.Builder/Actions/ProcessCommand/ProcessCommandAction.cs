using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.ProcessCommand
{
    public class ProcessCommandAction : ActionBase<JsonElement>
    {
        private readonly ISender _sender;
        private readonly IEnvelopeSerializer _envelopeSerializer;

        public ProcessCommandAction(ISender sender, IEnvelopeSerializer envelopeSerializer) : base(nameof(ProcessCommand))
        {
            _sender = sender;
            _envelopeSerializer = envelopeSerializer;
        }

        public override async Task ExecuteAsync(IContext context, JsonElement settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            string variable = null;

            if (settings.TryGetProperty(nameof(variable), out var variableToken))
            {
                variable = variableToken.GetString().Trim('"');
            }

            var command = GetCommandFromJsonDocument(settings);
            command.Id = EnvelopeId.NewId();

            var resultCommand = await _sender.ProcessCommandAsync(command, cancellationToken);

            if (string.IsNullOrWhiteSpace(variable)) return;

            var resultCommandJson = _envelopeSerializer.Serialize(resultCommand);
            await context.SetVariableAsync(variable, resultCommandJson, cancellationToken);
        }

        private static Command GetCommandFromJsonDocument(JsonElement settings)
        {
            var rawText = settings.GetRawText();
            return JsonConvert.DeserializeObject<Command>(rawText);
        }
    }
}