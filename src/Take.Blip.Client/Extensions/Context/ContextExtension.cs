using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using SmartFormat;

namespace Take.Blip.Client.Extensions.Context
{
    public class ContextExtension : ExtensionBase, IContextExtension
    {        
        public const string CONTEXTS = "/contexts";        
        public const string CONTEXT = "/contexts/{identity}";        
        public const string CONTEXT_VARIABLE = "/contexts/{identity}/{variableName}";
        public const string CONTEXT_GLOBAL_VARIABLE = "/contexts/{variableName}";

        public ContextExtension(ISender sender) : base(sender)
        {
        }

        public Task<T> GetVariableAsync<T>(Identity identity, string variableName, CancellationToken cancellationToken) where T : Document
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (variableName == null) throw new ArgumentNullException(nameof(variableName));

            var requestCommand = CreateGetCommandRequest(
                Smart.Format(CONTEXT, new { identity = Uri.EscapeDataString(identity.ToString()) }));

            return ProcessCommandAsync<T>(requestCommand, cancellationToken);
        }

        public Task SetVariableAsync<T>(Identity identity, string variableName, T document, CancellationToken cancellationToken,
            TimeSpan expiration = default(TimeSpan)) where T : Document
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (variableName == null) throw new ArgumentNullException(nameof(variableName));
            if (document == null) throw new ArgumentNullException(nameof(document));

            var uriPath = Smart.Format(
                CONTEXT_VARIABLE,
                new
                {
                    identity = Uri.EscapeDataString(identity.ToString()),
                    variableName
                });

            uriPath = AppendExpiration<T>(uriPath, expiration);

            var requestCommand = CreateSetCommandRequest(
                document,
                uriPath
                );

            return ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public Task SetGlobalVariableAsync<T>(string variableName, T document, CancellationToken cancellationToken,
            TimeSpan expiration = default(TimeSpan)) where T : Document
        {
            if (variableName == null) throw new ArgumentNullException(nameof(variableName));
            if (document == null) throw new ArgumentNullException(nameof(document));

            var uriPath = Smart.Format(
                CONTEXT_GLOBAL_VARIABLE,
                new
                {
                    variableName
                });

            uriPath = AppendExpiration<T>(uriPath, expiration);
            var requestCommand = CreateSetCommandRequest(document, uriPath);
            return ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public Task<T> DeleteVariableAsync<T>(Identity identity, string variableName, CancellationToken cancellationToken) where T : Document
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (variableName == null) throw new ArgumentNullException(nameof(variableName));
            
            var requestCommand = CreateDeleteCommandRequest(
                Smart.Format(
                    CONTEXT_VARIABLE,
                    new
                    {
                        identity = Uri.EscapeDataString(identity.ToString()),
                        variableName
                    }));

            return ProcessCommandAsync<T>(requestCommand, cancellationToken);
        }

        public Task<T> DeleteGlobalVariableAsync<T>(string variableName, CancellationToken cancellationToken) where T : Document
        {
            if (variableName == null) throw new ArgumentNullException(nameof(variableName));

            var requestCommand = CreateDeleteCommandRequest(
                Smart.Format(
                    CONTEXT_GLOBAL_VARIABLE,
                    new
                    {
                        variableName
                    }));

            return ProcessCommandAsync<T>(requestCommand, cancellationToken);
        }

        public Task<DocumentCollection> GetVariablesAsync(Identity identity, int skip = 0, int take = 100, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            var requestCommand = CreateGetCommandRequest(
                $"{Smart.Format(CONTEXT, new { identity = Uri.EscapeDataString(identity.ToString()) })}?{nameof(skip)}={skip}&{nameof(take)}={take}");
            return ProcessCommandAsync<DocumentCollection>(requestCommand, cancellationToken);

        }

        public Task<DocumentCollection> GetIdentitiesAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default(CancellationToken))
        {
            var requestCommand = CreateGetCommandRequest(
                $"{CONTEXTS}?{nameof(skip)}={skip}&{nameof(take)}={take}");
            return ProcessCommandAsync<DocumentCollection>(requestCommand, cancellationToken);
        }

        private static string AppendExpiration<T>(string uriPath, TimeSpan expiration) where T : Document
        {
            if (expiration != default(TimeSpan))
            {
                uriPath = $"{uriPath}?{nameof(expiration)}={expiration.TotalMilliseconds}";
            }
            return uriPath;
        }
    }
}