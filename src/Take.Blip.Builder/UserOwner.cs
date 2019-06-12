using System;
using Lime.Protocol;

namespace Take.Blip.Builder
{
    public sealed class UserOwner
    {
        public UserOwner(Identity userIdentity, Identity ownerIdentity)
        {
            UserIdentity = userIdentity ?? throw new ArgumentNullException(nameof(userIdentity));
            OwnerIdentity = ownerIdentity ?? throw new ArgumentNullException(nameof(ownerIdentity));
        }
    
        public Identity UserIdentity { get; }

        public Identity OwnerIdentity { get; }
        
        public void Deconstruct(out Identity userIdentity, out Identity ownerIdentity)
        {
            userIdentity = UserIdentity;
            ownerIdentity = OwnerIdentity;
        }
    }
}