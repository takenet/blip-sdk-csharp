using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using SmartFormat;

namespace Take.Blip.Client.Extensions.Context
{
    public class ContextExtension : ExtensionBase, IContextExtension
    {        
        public const string CONTEXTS = "/contexts";        
        public const string CONTEXT = "/contexts/{identity}";        
        public const string CONTEXT_VARIABLE = "/contexts/{identity}/{variableName}";

        public static readonly Identity GlobalIdentity = new Identity("*", Constants.DEFAULT_DOMAIN);

        public ContextExtension(ISender sender) : base(sender)
        {
        }

        public Task<T> GetVariableAsync<T>(Identity identity, string variableName, CancellationToken cancellationToken) where T : Document
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (variableName == null) throw new ArgumentNullException(nameof(variableName));

            var uriPath = GetVariableRequestUri(identity, variableName);
            var requestCommand = CreateGetCommandRequest(uriPath, id: $"disposable:{EnvelopeId.NewId()}");
            return ProcessCommandAsync<T>(requestCommand, cancellationToken);
        }

        public Task SetVariableAsync<T>(Identity identity, string variableName, T document, CancellationToken cancellationToken,
            TimeSpan expiration = default(TimeSpan)) where T : Document
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (variableName == null) throw new ArgumentNullException(nameof(variableName));
            if (document == null) throw new ArgumentNullException(nameof(document));

            var uriPath = GetVariableRequestUri(identity, variableName);
            uriPath = AppendExpiration<T>(uriPath, expiration);

            var requestCommand = CreateSetCommandRequest(
                document,
                uriPath
                );

            return ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public Task SetGlobalVariableAsync<T>(string variableName, T document, CancellationToken cancellationToken,
            TimeSpan expiration = default(TimeSpan)) where T : Document 
            => SetVariableAsync(GlobalIdentity, variableName, document, cancellationToken, expiration);

        public Task DeleteVariableAsync(Identity identity, string variableName, CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (variableName == null) throw new ArgumentNullException(nameof(variableName));

            var uriPath = GetVariableRequestUri(identity, variableName);
            var requestCommand = CreateDeleteCommandRequest(uriPath);

            return ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public Task DeleteGlobalVariableAsync(string variableName, CancellationToken cancellationToken) 
            => DeleteVariableAsync(GlobalIdentity, variableName, cancellationToken);

        public Task<DocumentCollection> GetVariablesAsync(Identity identity, int skip = 0, int take = 100, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            var requestCommand = CreateGetCommandRequest(
                $"{Smart.Format(CONTEXT, new { identity = Uri.EscapeDataString(identity.ToString()) })}?{nameof(skip)}={skip}&{nameof(take)}={take}");
            return ProcessCommandAsync<DocumentCollection>(requestCommand, cancellationToken);
        }

        public Task<DocumentCollection> GetIdentitiesAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default(CancellationToken))
        {
            var requestCommand = CreateGetCommandRequest($"{CONTEXTS}?{nameof(skip)}={skip}&{nameof(take)}={take}");
            return ProcessCommandAsync<DocumentCollection>(requestCommand, cancellationToken);
        }

        private static string GetVariableRequestUri(Identity identity, string variableName) 
            => Smart.Format(
                CONTEXT_VARIABLE,
                new
                {
                    identity = Uri.EscapeDataString(identity.ToString()),
                    variableName = Uri.EscapeDataString(variableName)
                });

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