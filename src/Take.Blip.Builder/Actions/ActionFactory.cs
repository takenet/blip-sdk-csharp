using System;
using System.Collections.Generic;
using System.Text;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions
{
    public class ActionFactory : IActionFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ActionFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IAction Create(string type, JObject settings)
        {
            switch (type)
            {
                case "SendMessage":
                    return new SendMessageAction((ISender)_serviceProvider.GetService(typeof(ISender)), settings.ToObject<Message>());
            }
            throw new ArgumentException("Unknown action type", nameof(type));
        }
    }
}
