using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions
{
    public class SendCommandAction : IAction
    {
        private readonly ISender _sender;

        public SendCommandAction(ISender sender)
        {
            _sender = sender;
        }

        public string Type => "SendCommand";

        public Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings), $"The settings are required for '{nameof(SendCommandAction)}' action");

            var command = settings.ToObject<Command>(LimeSerializerContainer.Serializer);
            command.Id = EnvelopeId.NewId();

            return _sender.SendCommandAsync(command, cancellationToken);
        }
    }
}