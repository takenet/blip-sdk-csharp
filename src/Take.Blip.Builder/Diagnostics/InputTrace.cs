using Lime.Protocol;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Take.Blip.Builder.Diagnostics
{
    [DataContract]
    public class InputTrace : Trace
    {
        public new static readonly MediaType MediaType = MediaType.Parse("application/vnd.blip.input-trace+json");

        public InputTrace() : base(MediaType)
        {
            States = new List<StateTrace>();
            InputActions = new List<ActionTrace>();
            OutputActions = new List<ActionTrace>();
        }

        [DataMember(Name = "owner")]
        public Identity Owner { get; set; }

        [DataMember(Name = "flowId")]
        public string FlowId { get; set; }

        [DataMember(Name = "user")]
        public string User { get; set; }

        [DataMember(Name = "input")]
        public string Input { get; set; }

        [DataMember(Name = "states")]
        public ICollection<StateTrace> States { get; set; }

        [DataMember(Name = "inputActions")]
        public ICollection<ActionTrace> InputActions { get; set; }

        [DataMember(Name = "outputActions")]
        public ICollection<ActionTrace> OutputActions { get; set; }
    }
}