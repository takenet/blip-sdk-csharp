using Lime.Messaging.Resources;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.Desk;
using Take.Blip.Builder.Diagnostics;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder
{
    public static class ContextExtensions
    {
        public const string CONTACT_KEY = "contact";
        public const string TICKET_KEY = "ticket";
        public const string CURRENT_ACTION_TRACE_KEY = "current-action-trace";

        public static Contact GetContact(this IContext context) 
            => GetValue<Contact>(context, CONTACT_KEY);

        public static void SetContact(this IContext context, Contact contact) 
            => SetValue(context, CONTACT_KEY, contact);

        public static void RemoveContact(this IContext context)
            => RemoveValue(context, CONTACT_KEY);
        
        public static Ticket GetTicket(this IContext context) 
            => GetValue<Ticket>(context, TICKET_KEY);

        public static void SetTicket(this IContext context, Ticket contact) 
            => SetValue(context, TICKET_KEY, contact);

        public static void RemoveTicket(this IContext context)
            => RemoveValue(context, TICKET_KEY);

        public static async Task<TicketContext> GetTicketVariableAsync(this IContext context, CancellationToken cancellationToken)
        {
            var ticketJson = await GetVariableAsync(context, TICKET_KEY, cancellationToken);
            if (string.IsNullOrEmpty(ticketJson))
                return null;

            return ParseFromJson<TicketContext>(ticketJson);
        }

        public static void SetTicketVariable(this IContext context, TicketContext ticketContext, CancellationToken cancellationToken)
        {
            var ticketJson = ParseFromObject(ticketContext);
            SetVariableAsync(context, TICKET_KEY, ticketJson, cancellationToken);
        }

        public static void RemoveTicketVariable(this IContext context, CancellationToken cancellationToken)
            => DeleteVariableAsync(context, TICKET_KEY, cancellationToken);

        public static ActionTrace GetCurrentActionTrace(this IContext context)
            => GetValue<ActionTrace>(context, CURRENT_ACTION_TRACE_KEY);

        public static void SetCurrentActionTrace(this IContext context, ActionTrace actionTrace)
            => SetValue(context, CURRENT_ACTION_TRACE_KEY, actionTrace);

        public static T GetValue<T>(this IContext context, string key) 
            => context.InputContext.TryGetValue(key, out var value) ? (T)value : default;

        public static void SetValue<T>(this IContext context, string key, T value)
        {
            RemoveValue(context, key);
            context.InputContext[key] = value;
        }

        public static void RemoveValue(this IContext context, string key) 
            => context.InputContext.Remove(key);

        public static Task<string> GetVariableAsync(this IContext context, string key, CancellationToken cancellationToken)
           => context.GetVariableAsync(key, cancellationToken);

        public static void SetVariableAsync(this IContext context, string key, string value, CancellationToken cancellationToken)
        {
            DeleteVariableAsync(context, key, cancellationToken);
            context.SetVariableAsync(key, value, cancellationToken);
        }

        public static void DeleteVariableAsync(this IContext context, string key, CancellationToken cancellationToken)
            => context.DeleteVariableAsync(key, cancellationToken);

        private static T ParseFromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static string ParseFromObject<T>(T objectToSerialize)
        {
            if (objectToSerialize == null) throw new ArgumentNullException(nameof(objectToSerialize));
            return JsonConvert.SerializeObject(objectToSerialize);
        }
    }
}
