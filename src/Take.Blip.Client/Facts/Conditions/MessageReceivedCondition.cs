using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Util;

namespace Take.Blip.Client.Facts.Conditions
{
    public class MessageReceivedCondition : Condition
    {
        public const string NAME = "messageReceived";

        private readonly MessageReceivedConditionFilter _filter;

        public MessageReceivedCondition(MessageReceivedConditionFilter filter)
            : base(NAME)
        {
            _filter = filter;
        }

        public override Task<bool> IsMatchAsync(object factProperty, IDictionary<string, object> context, CancellationToken cancellationToken)
        {
            if (!(factProperty is Message message))
            {
                throw new ArgumentException($"Invalid fact property for the current condition. Expected is Message, got {factProperty?.GetType()}.");
            }

            if (_filter == null) return TaskUtil.TrueCompletedTask;

            throw new NotImplementedException();
        }
    }
}
