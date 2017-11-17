using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Newtonsoft.Json.Linq;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions
{
    public class PauseAction : IAction
    {
        private readonly ISender _sender;

        private static readonly ChatState ComposingChatState = new ChatState()
        {
            State = ChatStateEvent.Composing
        };

        public PauseAction(ISender sender)
        {
            _sender = sender;
        }

        public string Type => "Pause";

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var pauseActionSettings = settings.ToObject<PauseActionSettings>();

            if (pauseActionSettings.SendComposing)
            {
                await _sender.SendMessageAsync(
                    ComposingChatState,
                    context.User.ToNode(),
                    cancellationToken);
            }

            await Task.Delay(pauseActionSettings.GetIntervalTimeSpan(), cancellationToken);
        }
    }
}
