using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Take.Blip.Client.WebTemplate.Old.Controllers;

namespace Take.Blip.Client.WebTemplate.Old
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var serviceContainer = MessagingHubConfig.StartAsync().Result;
            GlobalConfiguration.Configuration.DependencyResolver = new MessagingHubClientResolver(serviceContainer);
        }
    }
}
