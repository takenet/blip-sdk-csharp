namespace bot_flash_cards_blip_sdk_csharp
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Lime.Protocol;
    using System.Diagnostics;
    using Take.Blip.Client;
    
    public class ConstumedNotificationReceiver : INotificationReceiver
    {
        public async Task ReceiveAsync(Notification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine(notification.ToString());
        }
    }
}