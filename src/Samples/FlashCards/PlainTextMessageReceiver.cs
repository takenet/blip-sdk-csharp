namespace bot_flash_cards_blip_sdk_csharp
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Lime.Protocol;
    using System.Diagnostics;
    using Take.Blip.Client;
    using Take.Blip.Client.Session;
    using Lime.Messaging.Contents;

    /// <summary>
    /// Defines a class for handling messages. 
    /// This type must be registered in the application.json file in the 'messageReceivers' section.
    /// </summary>
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        
        private readonly Settings _settings;
        
        private StateMachine _stateMachine; 

        private readonly StateManager _stateManager;

        private ChatState _chatState = new ChatState { State = ChatStateEvent.Composing };

        public PlainTextMessageReceiver(ISender sender, Settings settings, IStateManager stateManager)
        {
            _sender = sender;
            _settings = settings;
            _stateMachine = new StateMachine(sender, stateManager);
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Trace.TraceInformation($"From: {message.From} \tContent: {message.Content}");
            await _stateMachine.RunAsync(message, cancellationToken, _chatState);
        }
    }
}
