using Blip.Ai.Bot.Monitoring.Logging.Models;
using Lime.Protocol;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Utils
{
    public static class BlipLogExtensions
    {
        private static readonly string ACTION_EXECUTION_EVENT_TYPE = "ActionExecution";
        private static readonly string STATE_EXECUTION_EVENT_TYPE = "StateExecution";
        private const string AGENT_STATE_PREFIX = "ai-agent:";

        public static LogInput ToActionLog(this IContext context, string title, JObject data, JObject sensitiveData = null)
        {
            var stateId = context.GetCurrentStateId();
            data = EnsureAgentFlag(stateId, data);

            return new LogInput
            {
                Title = title,
                EventType = ACTION_EXECUTION_EVENT_TYPE,
                Operation = string.Empty,
                StateId = stateId,
                Channel = context.Input.Message?.From?.Domain,
                IdMessage = context.Input.Message?.Id,
                From = context.UserIdentity?.ToString(),
                To = context.OwnerIdentity?.ToString(),
                OriginalFrom = context.Input.Message?.From,
                OriginalTo = context.Input.Message?.To,
                Data = data,
                FlowVersion = context?.Flow?.Version ?? 1,
                SensitiveData = sensitiveData,
                FlowId = context?.Flow?.Id
            };
        }

        public static LogInput ToStateLog(this IContext context, string title, string stateId, JObject data, JObject sensitiveData = null)
        {
            data = EnsureAgentFlag(stateId, data);

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
                SensitiveData = sensitiveData,
                FlowId = context?.Flow?.Id
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
            data = EnsureAgentFlag(stateId, data);

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
                FlowVersion = 1,
                FlowId = null
            };
        }

        private static JObject EnsureAgentFlag(string stateId, JObject data)
        {
            if (stateId == null || !stateId.StartsWith(AGENT_STATE_PREFIX))
            {
                return data;
            
            }
            data ??= new JObject();
            data["isAgent"] = true;
            return data;
        }
    }
}
