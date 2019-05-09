using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Take.Blip.Builder.Diagnostics
{
    public class StateTrace : Trace
    {
        public StateTrace()
        {
            InputActions = new List<ActionTrace>();
            OutputActions = new List<ActionTrace>();
            Outputs = new List<OutputTrace>();
        }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "inputActions")]
        public ICollection<ActionTrace> InputActions { get; set; }

        [DataMember(Name = "outputActions")]
        public ICollection<ActionTrace> OutputActions { get; set; }

        [DataMember(Name = "outputs")]
        public ICollection<OutputTrace> Outputs { get; set; }

        [DataMember(Name = "extensionData")]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }
}
