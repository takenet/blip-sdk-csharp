using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Models;
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

        public async Task<UserOwner> GetUserOwnerIdentitiesAsync<T>(T envelope, BuilderConfiguration builderConfiguration, CancellationToken cancellationToken) 
            where T : Envelope
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            if (envelope.From == null)
            {
                throw new ArgumentException("Envelope 'from' is required", nameof(envelope));
            }
            if (builderConfiguration == null) throw new ArgumentNullException(nameof(builderConfiguration));
            
            Identity userIdentity;
            Identity ownerIdentity;
            Tunnel tunnel = null;

            if (builderConfiguration.UseTunnelOwnerContext == true)
            {
                tunnel = await _tunnelExtension.TryGetTunnelAsync(envelope, cancellationToken);
            }

            if (tunnel != null)
            {
                userIdentity = tunnel.Originator;
                ownerIdentity = tunnel.Owner;
            }
            else if (builderConfiguration.OwnerIdentity != null)
            {
                userIdentity = envelope.From.ToIdentity();
                ownerIdentity = builderConfiguration.OwnerIdentity;
            }
            else
            {
                userIdentity = envelope.From.ToIdentity();
                ownerIdentity = _applicationIdentity;
            }

            return new UserOwner(userIdentity, ownerIdentity);
        }
    }
}