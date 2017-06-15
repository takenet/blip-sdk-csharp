using System;
using System.Runtime.Serialization;
using Lime.Protocol;

namespace Takenet.Iris.Messaging.Resources
{
    [DataContract]
    public class Schedule : Document
    {
        public const string MIME_TYPE = "application/vnd.iris.schedule+json";
        public static readonly MediaType MediaType = MediaType.Parse(MIME_TYPE);

        public const string WHEN = "when";
        public const string MESSAGE = "message";
        public const string STATUS = "status";

        public Schedule()
            : base(MediaType)
        {

        }

        [DataMember(Name = WHEN)]
        public DateTimeOffset When { get; set; }

        [DataMember(Name = MESSAGE)]
        public Message Message { get; set; }

        [DataMember(Name = STATUS)]
        public ScheduleStatus? Status { get; set; }
    }

    [DataContract]
    public enum ScheduleStatus
    {
        [EnumMember(Value = "scheduled")]
        Scheduled,
        [EnumMember(Value = "executed")]
        Executed,
        [EnumMember(Value = "canceled")]
        Canceled
    }
}
