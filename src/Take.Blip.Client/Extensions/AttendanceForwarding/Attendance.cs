using System.Runtime.Serialization;
using Lime.Protocol;

namespace Takenet.Iris.Messaging.Contents
{
    [DataContract]
    public class Attendance : Document
    {
        public static readonly string MimeType = "application/vnd.omni.attendance+json";
        public static readonly MediaType MediaType = MediaType.Parse(MimeType);

        /// <summary>
        /// Initializes a new instance of the <see cref="Attendance"/> class.
        /// </summary>
        public Attendance()
            : base(MediaType)
        {
        }

        /// <summary>
        /// Gets or sets the address of the attendant.
        /// </summary>
        /// <value>
        /// The attendand address.
        /// </value>
        [DataMember(Name = "attendant")]
        public Node Attendant { get; set; }

        /// <summary>
        /// Gets or sets the customer address.
        /// </summary>
        /// <value>
        /// The customer address.
        /// </value>
        [DataMember(Name = "customer")]
        public Node Customer { get; set; }

        /// <summary>
        /// Gets or sets the forwarded message content.
        /// </summary>
        /// <value>
        /// The message content.
        /// </value>
        [DataMember(Name = "content")]
        public DocumentContainer Content { get; set; }
    }
}
