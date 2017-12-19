using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions
{
    public class ProcessCommandAction : SenderActionBase, IAction
    {
        public ProcessCommandAction(ISender sender)
            : base(sender)
        {
        }

        public string Type => "ProcessCommand";

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings), $"The settings are required for '{nameof(ProcessCommandAction)}' action");

            string variable = null;

            if (settings.TryGetValue(nameof(variable), out var variableToken))
            {
                variable = variableToken.ToString().Trim('"');
            }

            var command = settings.ToObject<Command>(Serializer);
            command.Id = EnvelopeId.NewId();

            var resultCommand = await Sender.ProcessCommandAsync(command, cancellationToken);

            if (string.IsNullOrWhiteSpace(variable)) return;

            var resultCommandJson = JsonConvert.SerializeObject(resultCommand, JsonNetSerializer.Settings);
            await context.SetVariableAsync(variable, resultCommandJson, cancellationToken);
        }
    }
}