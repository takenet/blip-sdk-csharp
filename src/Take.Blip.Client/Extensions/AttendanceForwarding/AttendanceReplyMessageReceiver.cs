using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Extensions.AttendanceForwarding
{
    /// <summary>
    /// Automaticly forwards replies from an Blip App attendant
    /// to user that have sent a message
    /// </summary>
    [Obsolete("AttendanceReplyMessageReceiver is deprecated, please use HelpDeskExtension instead")]
    public class AttendanceReplyMessageReceiver : IMessageReceiver
    {
        private const string AttendantSettingsKey = "attendantIdentity";

        private readonly ISender _sender;
        private readonly IAttendanceExtension _attendanceExtension;
        private readonly IDictionary<string, object> _receiverSettings;
        private readonly Lazy<string> _operatorIdentity;

        public AttendanceReplyMessageReceiver(
            ISender sender,
            IAttendanceExtension attendanceExtension,
            IDictionary<string, object> receiverSettings)
        {
            _sender = sender;
            _attendanceExtension = attendanceExtension;
            _operatorIdentity =  new Lazy<string>(GetOperatorIdentityFromReceiverSettings, true);
            _receiverSettings = receiverSettings;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (_attendanceExtension.FromAttendant(message, GetOperatorIdentity(message)))
            {
                await _attendanceExtension.ForwardAttendantReplyAsync(message, cancellationToken);
            }
        }

        protected virtual string GetOperatorIdentity(Message message)
        {
            return _operatorIdentity.Value;
        }

        private string GetOperatorIdentityFromReceiverSettings()
        {
            if (!_receiverSettings.ContainsKey(AttendantSettingsKey))
                throw new InvalidOperationException($"{nameof(AttendanceReplyMessageReceiver)} must supply property '{AttendantSettingsKey}' inside receiver settings");
            return _receiverSettings[AttendantSettingsKey].ToString();
        }

    }
}
