using System;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Tunnel
{
    public sealed class TunnelInformation
    {
        public TunnelInformation(Identity owner, Node originator, Identity destination)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Originator = originator ?? throw new ArgumentNullException(nameof(originator));
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        }
        
        public Identity Owner { get; }
        
        public Node Originator { get; }
        
        public Identity Destination { get; }
    }
}