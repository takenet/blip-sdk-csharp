using System.Runtime.Serialization;

namespace Take.Blip.Builder.Actions.ThreadControl
{
    /// <summary>
    /// Defines the supported thread control operations for WhatsApp multi-partner management.
    /// </summary>
    [DataContract]
    public enum ThreadControlType
    {
        /// <summary>
        /// Passes thread control to the next partner in the routing chain or directly to a specified target role.
        /// </summary>
        [EnumMember(Value = "pass")]
        Pass,

        /// <summary>
        /// Takes thread control back to the current application.
        /// Requires the bot to be configured as the escalation partner in Meta.
        /// </summary>
        [EnumMember(Value = "take")]
        Take,

        /// <summary>
        /// Releases thread control from the current owner, returning the conversation thread to an idle state.
        /// </summary>
        [EnumMember(Value = "release")]
        Release
    }
}