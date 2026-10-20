using System.Runtime.Serialization;

namespace Take.Blip.Builder.Actions.ThreadControl
{
    /// <summary>
    /// Specifies the target roles recognized by Meta/WhatsApp for thread control handovers and routing.
    /// </summary>
    [DataContract]
    public enum TargetRole
    {
        /// <summary>
        /// Routes thread control to the customer service and human attendant partner.
        /// </summary>
        [EnumMember(Value = "customer_service")]
        CustomerService,

        /// <summary>
        /// Routes thread control to marketing workflows and campaign managers.
        /// </summary>
        [EnumMember(Value = "marketing")]
        Marketing,

        /// <summary>
        /// Routes thread control to transactional and utility service handlers.
        /// </summary>
        [EnumMember(Value = "utility")]
        Utility,

        /// <summary>
        /// Routes thread control to Click-to-WhatsApp Ads (CTWA) specific processors.
        /// </summary>
        [EnumMember(Value = "ctwa")]
        Ctwa,

        /// <summary>
        /// Routes thread control to autonomous AI agent platforms or assistants.
        /// </summary>
        [EnumMember(Value = "ai_agent")]
        AiAgent,

        /// <summary>
        /// Routes thread control directly to the designated escalation partner.
        /// </summary>
        [EnumMember(Value = "escalation")]
        Escalation
    }
}