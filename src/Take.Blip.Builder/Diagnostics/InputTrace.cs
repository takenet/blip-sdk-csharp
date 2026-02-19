using Lime.Protocol;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Take.Blip.Builder.Diagnostics
{
    /// <summary>  
    /// Represents the input trace for a monitoring event, including owner, flow, user, input, states, and actions.  
    /// </summary> 
    [DataContract]
    public class InputTrace : Trace
    {
        /// <summary>  
        /// The media type for input trace documents.  
        /// </summary> 
        public new static readonly MediaType MediaType = MediaType.Parse("application/vnd.blip.input-trace+json");

        /// <summary>
        /// Initializes a new instance of the <see cref="InputTrace"/> class with the specified media type.
        /// </summary>
        public InputTrace() : base(MediaType) { }

        /// <summary>  
        /// Gets or sets the unique identifier of the input trace.  
        /// </summary>  
        [DataMember(Name = "id")]
        public string? Id { get; set; }

        /// <summary>  
        /// Gets or sets the owner identifier associated with this trace.  
        /// </summary>  
        [DataMember(Name = "owner")]
        public Identity Owner { get; set; }

        /// <summary>  
        /// Gets or sets the flow identifier associated with this trace.  
        /// </summary> 
        [DataMember(Name = "flowId")]
        public string FlowId { get; set; }

        /// <summary>  
        /// Gets or sets the user identifier associated with this trace.  
        /// </summary>
        [DataMember(Name = "user")]
        public string User { get; set; }

        /// <summary>  
        /// Gets or sets the input value for this trace.  
        /// </summary> 
        [DataMember(Name = "input")]
        public string Input { get; set; }

        /// <summary>  
        /// Gets or sets the collection of state traces associated with this input.  
        /// </summary> 
        [DataMember(Name = "states")]
        public ICollection<StateTrace> States { get; set; }

        /// <summary>  
        /// Gets or sets the collection of input actions performed.  
        /// </summary> 
        [DataMember(Name = "inputActions")]
        public ICollection<ActionTrace> InputActions { get; set; }

        /// <summary>  
        /// Gets or sets the collection of output actions performed.  
        /// </summary> 
        [DataMember(Name = "outputActions")]
        public ICollection<ActionTrace> OutputActions { get; set; }

        /// <summary>
        /// Gets or sets the type of event ("AiAgents", "Builder").
        /// </summary>
        [DataMember(Name = "eventType")]
        public string? EventType { get; set; }

        [DataMember(Name = "afterStateChangedActions")]
        public ICollection<ActionTrace>? AfterStateChangedActions { get; set; }
    }
}