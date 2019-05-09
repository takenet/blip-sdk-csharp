using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Util;
using Serilog;
using Serilog.Core;
using Take.Blip.Client;
using Take.Blip.Client.Receivers;

namespace Programmatically
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new BlipClientBuilder()
                .UsingAccessKey("", "")
                .Build();

            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            var listener = new BlipChannelListener(client, true, Log.Logger);
            
            listener.AddMessageReceiver(
                new LambdaMessageReceiver(async (message, cancellationToken) =>
                {
                    Console.WriteLine("Message '{0}' received from '{1}': {2}", message.Id, message.From, message.Content);
                    await client.SendMessageAsync($"You said '{message.Content}'.", message.From, cancellationToken);                    
                }),
                m => Task.FromResult(m.Type == MediaType.TextPlain));

            listener.AddNotificationReceiver(
                new LambdaNotificationReceiver((notification, cancellationToken) =>
                {
                    Console.WriteLine("Notification '{0}' received from '{1}': {2}", notification.Id, notification.From, notification.Event);
                    return Task.CompletedTask;
                }));
            
            listener.AddCommandReceiver(
                new LambdaCommandReceiver((command, cancellationToken) =>
                {
                    Console.WriteLine("Command '{0}' received from '{1}': {2} {3}", command.Id, command.From, command.Method, command.Uri);
                    return Task.CompletedTask;
                }));

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {                                                
                await client.StartAsync(listener, cts.Token);                
            }

            Console.WriteLine("Client started. Press enter to stop.");
            Console.ReadLine();

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                await client.StopAsync(cts.Token);
            }

            Console.WriteLine("Client stopped. Press enter to exit.");
            Console.ReadLine();
        }
    }
}
