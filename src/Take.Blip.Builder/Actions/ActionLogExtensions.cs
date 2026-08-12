using System;
using Blip.Ai.Bot.Monitoring.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Actions
{
    public static class ActionLogExtensions
    {
        public static void LogExecution(this IAction action, IBlipLogger logger, IContext context, JObject data, JObject sensitiveData = null)
        {
            SetCurrentActionSource(context, data);
            logger.ActionExecution(context.ToActionLog(action.Type, data, sensitiveData));
        }

        public static void LogDelivery(this IAction action, IBlipLogger logger, IContext context, JObject data, JObject sensitiveData = null)
        {
            SetCurrentActionSource(context, data);
            logger.MessageDelivery(context.ToActionLog(action.Type, data, sensitiveData));
        }

        public static void LogError(this IAction action, IBlipLogger logger, IContext context, JObject data, Exception ex, JObject sensitiveData = null)
        {
            SetCurrentActionSource(context, data);
            logger.ErrorEvents(context.ToActionLog(action.Type, data, sensitiveData), ex);
        }

        private static void SetCurrentActionSource(this IContext context, JObject data)
        {
            var actionSource = context.GetCurrentActionSource();
            if (actionSource != null && data != null)
            {
                data = new JObject(data) { ["actionSource"] = actionSource };
            }

        }
    }
}
