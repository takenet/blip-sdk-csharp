using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions
{
    public class SendCommandAction : SenderActionBase, IAction
    {
        public SendCommandAction(ISender sender)
            : base(sender)
        {
        }

        public string Type => "SendCommand";

        public Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings), $"The settings are required for '{nameof(SendCommandAction)}' action");

            var command = settings.ToObject<Command>(Serializer);
            command.Id = EnvelopeId.NewId();

            return Sender.SendCommandAsync(command, cancellationToken);
        }
    }
}