using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace Take.Blip.Builder.Diagnostics
{
    public class ActionTrace : Trace
    {
        [DataMember(Name = "order")]
        public int Order { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "parsedSettings")]
        public JObject ParsedSettings { get; set; }
        
        [DataMember(Name = "continueOnError")]
        public bool ContinueOnError { get; set; }
    }
}
