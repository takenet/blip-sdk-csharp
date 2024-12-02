using System;
using System.Runtime.Serialization;
using Lime.Protocol;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Represents a function document in the system.
    /// </summary>
    public class Function : Document
    {
        public const string MIME_TYPE = "application/vnd.iris.builder.functions.functions+json";
        public static readonly MediaType MediaType = MediaType.Parse(MIME_TYPE);

        /// <summary>
        /// Initializes a new instance of the <see cref="Function"/> class.
        /// </summary>
        public Function() : base(MediaType) { }

        /// <summary>
        /// The function's id
        /// </summary>
        [DataMember(Name = "functionId")]
        public Guid FunctionId { get; set; }

        /// <summary>
        /// The function's name
        /// </summary>
        [DataMember(Name = "functionName")]
        public string FunctionName { get; set; }

        /// <summary>
        /// The function's descriptions
        /// </summary>
        [DataMember(Name = "functionDescription")]
        public string FunctionDescription { get; set; }

        /// <summary>
        /// The function's parameters
        /// </summary>
        [DataMember(Name = "functionParameters")]
        public string FunctionParameters { get; set; }

        /// <summary>
        /// The function's content
        /// </summary>
        [DataMember(Name = "functionContent")]
        public string FunctionContent { get; set; }

        /// <summary>
        /// The tenant id, owner of this function
        /// </summary>
        [DataMember(Name = "tentantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// The user who created this function
        /// </summary>
        [DataMember(Name = "userIdentity")]
        public string UserIdentity { get; set; }
    }
}
