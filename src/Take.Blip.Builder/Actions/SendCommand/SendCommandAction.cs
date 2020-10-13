using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.SendCommand
{
    public class SendCommandAction : ActionBase<JsonElement>
    {
        private readonly ISender _sender;

        public SendCommandAction(ISender sender) : base(nameof(SendCommand))
        {
            _sender = sender;
        }

        public override Task ExecuteAsync(IContext context, JsonElement settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var command = GetCommandFromJsonDocument(settings);
            command.Id = EnvelopeId.NewId();

            return _sender.SendCommandAsync(command, cancellationToken);
        }

        private static Command GetCommandFromJsonDocument(JsonElement settings)
        {
            var rawText = settings.GetRawText();
            return JsonConvert.DeserializeObject<Command>(rawText);
        }
    }
}