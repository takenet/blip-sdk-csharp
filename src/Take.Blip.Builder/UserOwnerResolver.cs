using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Models;
using Take.Blip.Client;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.Tunnel;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder
{
    public sealed class UserOwnerResolver : IUserOwnerResolver
    {
        private readonly ITunnelExtension _tunnelExtension;
        private readonly Identity _applicationIdentity;

        public UserOwnerResolver(ITunnelExtension tunnelExtension, Application application)
        {
            _tunnelExtension = tunnelExtension;
            _applicationIdentity = application.Identity;
        }

        public async Task<UserOwner> GetUserOwnerIdentitiesAsync(Message message, BuilderConfiguration builderConfiguration, CancellationToken cancellationToken)
        {
            Identity userIdentity;
            Identity ownerIdentity;
            Tunnel tunnel = null;

            if (builderConfiguration.UseTunnelOwnerContext == true)
            {
                tunnel = await _tunnelExtension.TryGetTunnelForMessageAsync(message, cancellationToken);
            }

            if (tunnel != null)
            {
                userIdentity = tunnel.Originator.ToIdentity();
                ownerIdentity = tunnel.Owner;
            }
            else if (builderConfiguration.OwnerIdentity != null)
            {
                userIdentity = message.From.ToIdentity();
                ownerIdentity = builderConfiguration.OwnerIdentity;
            }
            else
            {
                userIdentity = message.From.ToIdentity();
                ownerIdentity = _applicationIdentity;
            }

            return new UserOwner(userIdentity, ownerIdentity);
        }
    }
}