using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Tunnel
{
    public sealed class TunnelInformation
    {
        public TunnelInformation(Identity owner, Node originator)
        {
            Owner = owner;
            Originator = originator;
        }
        
        public Identity Owner { get; }
        
        public Node Originator { get; }
    }
}