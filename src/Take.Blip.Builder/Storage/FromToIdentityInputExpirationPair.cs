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

        public override string ToString() => $"inputexpiration-loop-counter:{FromIdentity}:{ToIdentity}";
    }
}
