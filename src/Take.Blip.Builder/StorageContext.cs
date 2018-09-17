using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Network;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Storage;
using Take.Blip.Builder.Variables;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a context that queries directly in the storage engine.
    /// </summary>
    public class StorageContext : ContextBase
    {
        private readonly IOwnerCallerNameDocumentMap _ownerCallerNameDocumentMap;

        public StorageContext(
            Identity user,
            Identity application,
            LazyInput input,
            Flow flow,
            IEnumerable<IVariableProvider> variableProviders,
            IOwnerCallerNameDocumentMap ownerCallerNameDocumentMap)
            : base(user, application, input, flow, variableProviders)
        {
            _ownerCallerNameDocumentMap = ownerCallerNameDocumentMap;
        }

        public override async Task SetVariableAsync(string name, string value, CancellationToken cancellationToken, TimeSpan expiration = default(TimeSpan))
        {
            var storageDocument = new StorageDocument
            {
                Type = PlainText.MediaType,
                Document = value
            };

            if (expiration != default(TimeSpan))
            {
                storageDocument.Expiration = DateTimeOffset.UtcNow.Add(expiration);
            }

            var key = OwnerCallerName.Create(Application, User, name.ToLowerInvariant());

            if (!await _ownerCallerNameDocumentMap.TryAddAsync(key, storageDocument, true))
            {
                throw new LimeException(ReasonCodes.COMMAND_PROCESSING_ERROR, "An unexpected error occurred while storing the document");
            }

            if (expiration != default(TimeSpan))
            {
                await _ownerCallerNameDocumentMap.SetRelativeKeyExpirationAsync(key, expiration);
            }
        }

        public override Task DeleteVariableAsync(string name, CancellationToken cancellationToken)
        {
            return _ownerCallerNameDocumentMap.TryRemoveAsync(OwnerCallerName.Create(Application, User, name.ToLowerInvariant()));
        }

        protected override async Task<string> GetContextVariableAsync(string name, CancellationToken cancellationToken)
        {
            var key = OwnerCallerName.Create(Application, User, name.ToLowerInvariant());
            var storageDocument = await _ownerCallerNameDocumentMap.GetValueOrDefaultAsync(key);

            return storageDocument?.Document;
        }
    }
}
