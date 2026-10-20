using System.Runtime.Serialization;
using Lime.Protocol;

namespace Take.Blip.Builder.Actions.ThreadControl
{
    /// <summary>
    /// Represents the LIME resource document for executing WhatsApp thread control operations 
    /// (such as take, pass, and release) within multi-partner setups.
    /// </summary>
    [DataContract]
    public class ThreadControlCommandResource : Document
    {
        /// <summary>
        /// The MIME type string representation for WhatsApp thread control commands.
        /// </summary>
        public const string MIME_TYPE = "application/vnd.iris.whatsapp.thread-control+json";

        /// <summary>
        /// The parsed LIME <see cref="MediaType"/> for WhatsApp thread control commands.
        /// </summary>
        public static readonly MediaType MediaType = MediaType.Parse(MIME_TYPE);

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadControlCommandResource"/> class.
        /// </summary>
        public ThreadControlCommandResource() : base(MediaType)
        {
        }

        /// <summary>
        /// Gets or sets the thread control operation type.
        /// Supported values: "take", "release", or "pass".
        /// </summary>
        [DataMember(Name = "controlType")]
        public ThreadControlType ControlType { get; set; }

        /// <summary>
        /// Gets or sets the target contact identity (e.g., phone number) for the thread control command.
        /// </summary>
        [DataMember(Name = "userIdentity")]
        public Identity UserIdentity { get; set; }

        /// <summary>
        /// Gets or sets the target role when passing thread control (e.g., "customer_service", "ai_agent").
        /// Mandatory for targeted "pass" operations; must be omitted for "take" and "release".
        /// </summary>
        [DataMember(Name = "targetRole", EmitDefaultValue = false)]
        public TargetRole? TargetRole { get; set; }

        /// <summary>
        /// Gets or sets optional contextual metadata or notes associated with the thread control operation.
        /// </summary>
        [DataMember(Name = "metadata", EmitDefaultValue = false)]
        public string Metadata { get; set; }
    }
}