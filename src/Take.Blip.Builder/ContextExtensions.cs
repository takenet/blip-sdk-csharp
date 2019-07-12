using Lime.Messaging.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Builder
{
    public static class ContextExtensions
    {
        private const string CONTACT_KEY = "contact";

        public static Contact GetContact(this IContext context)
        {
            return context.InputContext.TryGetValue(CONTACT_KEY, out var contact) ? (Contact)contact : null;
        }

        public static void SetContact(this IContext context, Contact contact)
        {
            RemoveContact(context);
            context.InputContext[CONTACT_KEY] = contact;
        }

        public static void RemoveContact(this IContext context)
        {
            context.InputContext.Remove(CONTACT_KEY);
        }
    }
}
