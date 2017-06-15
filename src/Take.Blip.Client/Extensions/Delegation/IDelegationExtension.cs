using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Delegation
{
    public interface IDelegationExtension
    {
        Task DelegateAsync(Identity target, EnvelopeType[] envelopeTypes = null, CancellationToken cancellationToken = default(CancellationToken));

        Task UndelegateAsync(Identity target, EnvelopeType[] envelopeTypes = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
