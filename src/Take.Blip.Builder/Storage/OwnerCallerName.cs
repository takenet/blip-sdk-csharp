using System;
using System.Runtime.Serialization;
using Lime.Protocol;

namespace Take.Blip.Builder.Storage
{
    [DataContract]
    public class OwnerCallerName : OwnerCaller
    {
        [DataMember]
        public string Name { get; set; }

        public override string ToString() => $"{base.ToString()}{SEPARATOR}{Name}";

        public static OwnerCallerName Create(Identity owner, Identity caller, string name) =>
            new OwnerCallerName
            {
                Owner = owner,
                Caller = caller,
                Name = name,
            };

        public static new OwnerCallerName Parse(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            var values = s.Split(SEPARATOR);
            return Create(values[0], values[1], values[2]);
        }
    }
}
