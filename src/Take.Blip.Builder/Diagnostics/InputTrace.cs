using System.Collections.Generic;

namespace Take.Blip.Builder.Diagnostics
{
    public class InputTrace : Trace
    {
        public InputTrace()
        {
            States = new List<StateTrace>();
        }

        public string FlowId { get; set; }

        public string User { get; set; }

        public string Input { get; set; }

        public ICollection<StateTrace> States { get; set; }
    }
}
