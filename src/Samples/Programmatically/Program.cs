using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Util;

namespace Take.Blip.Client.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args, CancellationToken.None).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args, CancellationToken cancellationToken)
        {
            var client = new BlipClientBuilder()
                .UsingAccessKey("animaisdemo", "V01WNEJtVDBvRVRod1Bycm11Umw=")
                .Build();

            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            await client.StartAsync(
                m =>
                {
                    Console.WriteLine("Message '{0}' received from '{1}': {2}", m.Id, m.From, m.Content);
                    return TaskUtil.TrueCompletedTask;
                },
                n =>
                {
                    Console.WriteLine("Notification '{0}' received from '{1}': {2}", n.Id, n.From, n.Event);
                    return TaskUtil.TrueCompletedTask;
                },
                c =>
                {
                    Console.WriteLine("Command '{0}' received from '{1}': {2} {3}", c.Id, c.From, c.Method, c.Uri);
                    return TaskUtil.TrueCompletedTask;
                },
                cancellationToken);

            Console.WriteLine("Client started. Press enter to stop.");
            Console.ReadLine();

            await client.StopAsync(cancellationToken);

            Console.WriteLine("Client stopped. Press enter to exit.");
            Console.ReadLine();
        }
    }
}