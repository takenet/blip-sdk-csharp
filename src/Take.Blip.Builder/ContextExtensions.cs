using Lime.Messaging.Resources;
using Take.Blip.Builder.Diagnostics;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder
{
    public static class ContextExtensions
    {
        public const string CONTACT_KEY = "contact";
        public const string TICKET_KEY = "ticket";
        public const string CURRENT_ACTION_TRACE_KEY = "current-action-trace";
        public const string CURRENT_STATE_ID_KEY = "current-state-id";
        public const string CURRENT_ACTION_SOURCE_KEY = "current-action-source";

        public static Contact GetContact(this IContext context) =>
            GetValue<Contact>(context, CONTACT_KEY);

        public static void SetContact(this IContext context, Contact contact) =>
            SetValue(context, CONTACT_KEY, contact);

        public static void RemoveContact(this IContext context) =>
            RemoveValue(context, CONTACT_KEY);

        public static Ticket GetTicket(this IContext context) =>
            GetValue<Ticket>(context, TICKET_KEY);

        public static void SetTicket(this IContext context, Ticket contact) =>
            SetValue(context, TICKET_KEY, contact);

        public static ActionTrace GetCurrentActionTrace(this IContext context) =>
            GetValue<ActionTrace>(context, CURRENT_ACTION_TRACE_KEY);

        public static void SetCurrentActionTrace(this IContext context, ActionTrace actionTrace) =>
            SetValue(context, CURRENT_ACTION_TRACE_KEY, actionTrace);

        public static string GetCurrentStateId(this IContext context) =>
            GetValue<string>(context, CURRENT_STATE_ID_KEY);

        public static void SetCurrentStateId(this IContext context, string stateId) =>
            SetValue(context, CURRENT_STATE_ID_KEY, stateId);

        public static string GetCurrentActionSource(this IContext context) =>
            GetValue<string>(context, CURRENT_ACTION_SOURCE_KEY);

        public static void SetCurrentActionSource(this IContext context, string actionSource) =>
            SetValue(context, CURRENT_ACTION_SOURCE_KEY, actionSource);

        public static void RemoveTicket(this IContext context) => RemoveValue(context, TICKET_KEY);

        public static T GetValue<T>(this IContext context, string key) =>
            context.InputContext.TryGetValue(key, out var value) ? (T)value : default;

        public static void SetValue<T>(this IContext context, string key, T value)
        {
            RemoveValue(context, key);
            context.InputContext[key] = value;
        }

        public static void RemoveValue(this IContext context, string key) =>
            context.InputContext.Remove(key);
    }
}
