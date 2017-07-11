using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;

namespace Take.Blip.Client
{
    internal sealed class MessageReceivedHandler : EnvelopeReceivedHandler<Message>
    {
        private readonly bool _autoNotifiy;
        private readonly ISender _sender;

        public MessageReceivedHandler(ISender sender, bool autoNotifiy, EnvelopeReceiverManager registrar)
            : base(registrar)
        {
            _autoNotifiy = autoNotifiy;
            _sender = sender;
        }

        protected override async Task CallReceiversAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                if (_autoNotifiy)
                {
                    await _sender.SendNotificationAsync(message.ToReceivedNotification(), cancellationToken);
                }

                await base.CallReceiversAsync(message, cancellationToken);

                if (_autoNotifiy)
                {
                    await _sender.SendNotificationAsync(message.ToConsumedNotification(), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Reason reason = null;
                if (ex is LimeException)
                {
                    reason = ((LimeException)ex).Reason;
                }
                else
                {
                    reason = new Reason
                    {
                        Code = ReasonCodes.APPLICATION_ERROR,
                        Description = ex.Message
                    };
                }

                if (_autoNotifiy)
                {
                    await _sender.SendNotificationAsync(message.ToFailedNotification(reason), CancellationToken.None);
                }
                throw;
            }
        }
    }
}