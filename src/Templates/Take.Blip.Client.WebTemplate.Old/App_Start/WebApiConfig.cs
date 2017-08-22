using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Take.Blip.Client.WebTemplate.Old.Models;

namespace Take.Blip.Client.WebTemplate.Old
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.BindParameter(typeof(Message), new EnvelopeModelBinder());
            config.BindParameter(typeof(Notification), new EnvelopeModelBinder());
        }
    }
}
