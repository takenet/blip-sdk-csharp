using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Delegation
{
    public class DelegationExtension : ExtensionBase, IDelegationExtension
    {
        const string DELEGATIONS_URI = "/delegations";


        public DelegationExtension(ISender sender)
            : base(sender)
        {

        }

        public async Task DelegateAsync(Identity target, EnvelopeType[] envelopeTypes = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (target == null) throw new ArgumentNullException(nameof(target));                        
            var requestCommand = new Command()
            {
                Method = CommandMethod.Set,
                Uri = new LimeUri(DELEGATIONS_URI),
                Resource = new Lime.Messaging.Resources.Delegation()
                {
                    EnvelopeTypes = envelopeTypes,
                    Target = target.ToNode()
                }
            };

            await ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public Task UndelegateAsync(Identity target, EnvelopeType[] envelopeTypes = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }
    }
}