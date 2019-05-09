using System.Runtime.Serialization;

namespace Take.Blip.Builder.Diagnostics
{
    public class OutputTrace : Trace
    {
        [DataMember(Name = "stateId")]
        public string StateId { get; set; }

        [DataMember(Name = "conditionsCount")]
        public int ConditionsCount { get; set; }
    }
}
