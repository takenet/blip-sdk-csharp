using System;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Actions
{
    public static class ActionLogExtensions
    {
        public static void LogExecution(this IAction action, IBlipLogger logger, IContext context, JObject data)
        {
            logger.ActionExecution(context.ToActionLog(action.Type, data));
        }

        public static void LogError(this IAction action, IBlipLogger logger, IContext context, JObject data, Exception ex)
        {
            logger.ErrorEvents(context.ToActionLog(action.Type, data), ex);
        }
    }
}
