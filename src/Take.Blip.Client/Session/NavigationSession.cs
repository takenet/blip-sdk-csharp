using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Lime.Protocol;

namespace Take.Blip.Client.Session
{
    /// <summary>
    /// Defines a session storage document.
    /// </summary>
    /// <seealso cref="Lime.Protocol.Document" />
    [DataContract]
    public class NavigationSession : Document
    {
        private static readonly MediaType MediaType = MediaType.Parse("application/vnd.takenet.navigation-session+json");

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSession"/> class.
        /// </summary>
        public NavigationSession()
            : base(MediaType)
        {

        }

        /// <summary>
        /// Gets or sets the session creation date.
        /// </summary>
        /// <value>
        /// The creation.
        /// </value>
        [DataMember(Name = "creation")]
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Gets or sets the session variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        [DataMember(Name = "variables")]
        public Dictionary<string, string> Variables { get; set; }
    }
}