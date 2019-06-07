using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Tunnel
{
    public sealed class TunnelInformation
    {
        public TunnelInformation(Identity owner, Node originator, Identity destination)
        {
            Owner = owner;
            Originator = originator;
            Destination = destination;
        }
        
        public Identity Owner { get; }
        
        public Node Originator { get; }
        
        public Identity Destination { get; }
    }
}