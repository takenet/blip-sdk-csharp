using System;
using System.Collections.Generic;
using System.Text;
using Lime.Protocol;

namespace Take.Blip.Builder.Storage
{
    public class FromToIdentityInputExpirationPair
    {
        public Identity FromIdentity { get; set; }

        public Identity ToIdentity { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (FromToIdentityInputExpirationPair)obj;
            return FromIdentity.Equals(other.FromIdentity) && ToIdentity.Equals(other.ToIdentity);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(FromIdentity, ToIdentity);
        }
    }
}
