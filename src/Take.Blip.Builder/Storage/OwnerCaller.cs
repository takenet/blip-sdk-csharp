using Lime.Protocol;
using System;
using System.Runtime.Serialization;

namespace Take.Blip.Builder.Storage
{
    /// <summary>
    /// Represents a identity pair.
    /// </summary>
    [DataContract]
    public class OwnerCaller
    {
        public const char SEPARATOR = ':';

        [DataMember]
        public Identity Owner { get; set; }

        [DataMember]
        public Identity Caller { get; set; }

        public override string ToString() => $"{Owner}{SEPARATOR}{Caller}";

        public static OwnerCaller Create(Identity owner, Identity caller)
            => new OwnerCaller
            {
                Owner = owner,
                Caller = caller
            };

        public static OwnerCaller Parse(string s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var values = s.Split(SEPARATOR);
            return Create(values[0], values[1]);
        }

        public override bool Equals(object obj)
            => obj != null && ToString().Equals(obj.ToString(), StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode() => ToString().GetHashCode();
    }
}
