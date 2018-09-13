using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Diagnostics
{
    public class StateTrace : Trace
    {
        public StateTrace()
        {
            InputActions = new List<ActionTrace>();
            OutputActions = new List<ActionTrace>();
        }

        public string Id { get; set; }

        public ICollection<ActionTrace> InputActions { get; set; }

        public ICollection<ActionTrace> OutputActions { get; set; }

        public ICollection<OutputTrace> Outputs { get; set; }

        public IDictionary<string, JToken> ExtensionData { get; set; }
    }
}
