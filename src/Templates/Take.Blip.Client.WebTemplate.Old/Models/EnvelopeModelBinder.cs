using Lime.Protocol.Serialization.Newtonsoft;
using System;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Take.Blip.Client.WebTemplate.Old.Models
{
    public class EnvelopeModelBinder : IModelBinder
    {
        private readonly JsonNetSerializer _serializer;

        public EnvelopeModelBinder()
        {
            _serializer = new JsonNetSerializer();
        }

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var json = actionContext.Request.Content.ReadAsStringAsync().Result;
            bindingContext.Model = _serializer.Deserialize(json);
            return bindingContext.Model != null;        
        }
    }
}