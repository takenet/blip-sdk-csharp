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
        private readonly UnitOfWork _unitOfWork;

        /// <inheritdoc />
        public ExtensionContext(
            Identity user,
            Identity application,
            LazyInput input,
            Flow flow,
            IEnumerable<IVariableProvider> variableProviders,
            IContextExtension contextExtension,
            bool useUnitOfWork)
            : base (user, application, input, flow, variableProviders)
        {
            _contextExtension = contextExtension;
            if(useUnitOfWork)
            {
                _unitOfWork = new UnitOfWork(contextExtension, UserIdentity);
            }
        }

        /// <inheritdoc />
        public override async Task SetVariableAsync(string name, string value, CancellationToken cancellationToken, TimeSpan expiration = default)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if(_unitOfWork != null)
            {
                _unitOfWork.SetVariable(name, value, expiration);
                return;
            }

            await _contextExtension.SetTextVariableAsync(UserIdentity, name.ToLowerInvariant(), value, cancellationToken, expiration);
        }

        /// <inheritdoc />
        public override async Task DeleteVariableAsync(string name, CancellationToken cancellationToken)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if(_unitOfWork != null)
            {
                _unitOfWork.DeleteVariable(name);
                return;
            }

            try
            {
                await _contextExtension.DeleteVariableAsync(UserIdentity, name.ToLowerInvariant(), cancellationToken);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND) { }
        }

        /// <inheritdoc />
        public override async Task<string> GetContextVariableAsync(string name, CancellationToken cancellationToken)
        {
            if(_unitOfWork != null)
            {
                return await _unitOfWork.GetContextVariableAsync(name, cancellationToken);
            }

            try
            {
                return await _contextExtension.GetTextVariableAsync(UserIdentity, name, cancellationToken);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public override async Task CommitChangesAsync(CancellationToken cancellationToken)
        {
            if (_unitOfWork != null)
            {
                await _unitOfWork.CommitAsync(cancellationToken);
            }
        }
    }
}