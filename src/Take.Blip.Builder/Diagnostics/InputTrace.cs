using Lime.Protocol;
using System.Collections.Generic;

namespace Take.Blip.Builder.Diagnostics
{
    public class InputTrace : Trace
    {
        public new static readonly MediaType MediaType = MediaType.Parse("application/vnd.takenet.input-trace+json");

        public InputTrace() : base(MediaType)
        {
            States = new List<StateTrace>();
        }

        public string FlowId { get; set; }

        public string User { get; set; }

        public string Input { get; set; }

        public ICollection<StateTrace> States { get; set; }
    }
}