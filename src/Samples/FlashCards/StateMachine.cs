namespace bot_flash_cards_blip_sdk_csharp
{
    using Lime.Protocol;
    using System.Threading;
    using System.Threading.Tasks;
    using Take.Blip.Client;
    using Take.Blip.Client.Session;
    using Take.Blip.Client.Extensions.Bucket;
    using System;
    using Lime.Messaging.Contents;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Globalization;

    public class StateMachine
    {
        private readonly IStateManager _stateManager;
       
        private readonly ISender _sender;

        private readonly IDictionary<string, Game> _games;

        public StateMachine(ISender sender, IStateManager stateManager)
        {
            _sender = sender;
            _stateManager = stateManager;
            _games = new Dictionary<string, Game>();
        }

        public async Task<string> VerifyStateAsync(Message message, CancellationToken cancellationToken)
        {
            var currentState = await _stateManager.GetStateAsync(message.From.ToIdentity(), cancellationToken);
            return currentState == "default" ? "state-one" : currentState;
        }

        // This method was taken from the link https://stackoverflow.com/questions/3288114/what-does-nets-string-normalize-do       
        public static string RemoveAccents(string input) 
        {
            return new string(
                input
                .Normalize(System.Text.NormalizationForm.FormD)
                .ToCharArray()
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
        }

        public async Task RunAsync(Message message, CancellationToken cancellationToken, ChatState chatState)
        {
            Game game = _games.ContainsKey(message.From.ToIdentity().ToString()) ? game = _games[message.From.ToIdentity().ToString()] : new Game();

            switch (await VerifyStateAsync(message, cancellationToken))
            {
                case "state-one":
                    game.People = Reader.Run();
                    game.Answers = new List<Answer>();

                    await _sender.SendMessageAsync(chatState, message.From, cancellationToken);
                    Thread.Sleep(1000);
                    await _sender.SendMessageAsync("Hi! Let's start!", message.From, cancellationToken);
                    Thread.Sleep(1000);
                    await _sender.SendMessageAsync(chatState, message.From, cancellationToken);
                    Thread.Sleep(1000);
                    await _sender.SendMessageAsync("What's your name?", message.From, cancellationToken);

                    await _stateManager.SetStateAsync(message.From, "state-two", cancellationToken);
                break;

                case "state-two":
                    game.Player = message.Content.ToString();
                    
                    await _sender.SendMessageAsync(chatState, message.From, cancellationToken);
                    Thread.Sleep(1000);
                    await _sender.SendMessageAsync($"How many questions do you want to your game? The maximum is {game.People.Count}.", message.From, cancellationToken);

                    await _stateManager.SetStateAsync(message.From, "state-three", cancellationToken);
                break;

                case "state-three":
                    int questions;

                    if(Int32.TryParse(message.Content.ToString(), out questions) && questions <= game.People.Count && questions > 0)
                    {
                        game.Questions = Convert.ToInt32(message.Content.ToString());

                        await _sender.SendMessageAsync(chatState, message.From, cancellationToken);
                        Thread.Sleep(1000);
                        await _sender.SendMessageAsync(game.Run(), message.From, cancellationToken);

                        game.Questions--;

                        await _stateManager.SetStateAsync(message.From, "state-four", cancellationToken);
                    }
                    else
                    {
                        await _sender.SendMessageAsync(chatState, message.From, cancellationToken);
                        Thread.Sleep(1000);
                        await _sender.SendMessageAsync("Please, enter a valid number.", message.From, cancellationToken);

                        await _stateManager.SetStateAsync(message.From, "state-three", cancellationToken);
                    }
                break;

                case "state-four":
                    game.ProccessAnswer(RemoveAccents(message.Content.ToString()));

                    if (game.Questions > 0)
                    {
                        await _sender.SendMessageAsync(chatState, message.From, cancellationToken);
                        Thread.Sleep(1000);
                        await _sender.SendMessageAsync(game.Run(), message.From, cancellationToken);
                        
                        game.Questions--;
                    }
                    else
                    {
                        await _sender.SendMessageAsync(chatState, message.From, cancellationToken);
                        Thread.Sleep(1000);
                        await _sender.SendMessageAsync($"Yay! {game.Player}, your result is: {game.ProccesResult()}.", message.From, cancellationToken);
                        Thread.Sleep(1000);
                        await _sender.SendMessageAsync(chatState, message.From, cancellationToken);
                        Thread.Sleep(1000);
                        await _sender.SendMessageAsync(game.ShowErros(), message.From, cancellationToken);

                        await _stateManager.SetStateAsync(message.From, "state-one", cancellationToken); 
                    }                 
                break;

                default:
                    await _sender.SendMessageAsync(chatState, message.From, cancellationToken);
                    Thread.Sleep(1000);
                    await _sender.SendMessageAsync("Sorry, I don't understand.", message.From, cancellationToken);

                    await _stateManager.SetStateAsync(message.From, "state-one", cancellationToken);
                break;
            }

            _games[message.From.ToIdentity().ToString()] = game;
        }
    }
}