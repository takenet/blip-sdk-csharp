using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;

namespace Take.Blip.Builder.Utils
{
    public static class BlipLogExtensions
    {
        private static readonly string ACTION_EXECUTION_EVENT_TYPE = "ActionExecution";
        private static readonly string STATE_EXECUTION_EVENT_TYPE = "StateExecution";

        public static LogInput ToActionLog(this IContext context, string title, JObject data, JObject sensitiveData = null)
        {
            return new LogInput
            {
                Title = title,
                EventType = ACTION_EXECUTION_EVENT_TYPE,
                Operation = string.Empty,
                StateId = context.GetCurrentStateId(),
                Channel = context.Input.Message?.From?.Domain,
                IdMessage = context.Input.Message?.Id,
                From = context.UserIdentity?.ToString(),
                To = context.OwnerIdentity?.ToString(),
                OriginalFrom = context.Input.Message?.From,
                OriginalTo = context.Input.Message?.To,
                Data = data,
                FlowVersion = context?.Flow?.Version ?? 1,
                SensitiveData = sensitiveData,
            };
        }

        public static LogInput ToStateLog(this IContext context, string title, string stateId, JObject data, JObject sensitiveData = null)
        {
            return new LogInput
            {
                Title = title,
                EventType = STATE_EXECUTION_EVENT_TYPE,
                Operation = string.Empty,
                StateId = stateId,
                Channel = context?.Input?.Message?.From?.Domain,
                IdMessage = context?.Input?.Message?.Id,
                From = context?.UserIdentity?.ToString(),
                To = context?.OwnerIdentity?.ToString(),
                OriginalFrom = context?.Input?.Message?.From,
                OriginalTo = context?.Input?.Message?.To,
                Data = data,
                FlowVersion = context?.Flow?.Version ?? 1,
                SensitiveData = sensitiveData
            };
        }

        public static LogInput ToStateLog(
            string title,
            string stateId,
            Message message,
            Identity userIdentity,
            Identity ownerIdentity,
            JObject data)
        {
            return new LogInput
            {
                Title = title,
                EventType = STATE_EXECUTION_EVENT_TYPE,
                Operation = string.Empty,
                StateId = stateId,
                Channel = message?.From?.ToNode().Domain,
                IdMessage = message?.Id,
                From = userIdentity?.ToString(),
                To = ownerIdentity?.ToString(),
                OriginalFrom = message?.From,
                OriginalTo = message?.To,
                Data = data,
                FlowVersion = 1
            };
        }
    }
}
