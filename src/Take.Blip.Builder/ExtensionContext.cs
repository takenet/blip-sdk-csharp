using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Variables;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a context that uses the BLiP SDK context extension.
    /// </summary>
    public class ExtensionContext : ContextBase
    {
        private readonly IContextExtension _contextExtension;

        public ExtensionContext(
            Identity user,
            Identity application,
            LazyInput input,
            Flow flow,
            IEnumerable<IVariableProvider> variableProviders,
            IContextExtension contextExtension
            )
            : base (user, application, input, flow, variableProviders)
        {
            _contextExtension = contextExtension;
        }

        public override Task SetVariableAsync(string name, string value, CancellationToken cancellationToken, TimeSpan expiration = default(TimeSpan))
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return _contextExtension.SetTextVariableAsync(UserIdentity, name.ToLowerInvariant(), value, cancellationToken, expiration);
        }

        public override async Task DeleteVariableAsync(string name, CancellationToken cancellationToken)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            try
            {
                await _contextExtension.DeleteVariableAsync(UserIdentity, name.ToLowerInvariant(), cancellationToken);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND) { }
        }

        public override async Task<string> GetContextVariableAsync(string name, CancellationToken cancellationToken)
        {
            try
            {
                return await _contextExtension.GetTextVariableAsync(UserIdentity, name, cancellationToken);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return null;
            }
        }
    }
}