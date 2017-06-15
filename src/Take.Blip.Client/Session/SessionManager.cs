using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Client.Session
{
    public class SessionManager : ISessionManager
    {
        private const string CULTURE_KEY = "#culture";

        private readonly IBucketExtension _bucketExtension;
        private static readonly TimeSpan SessionExpiration = TimeSpan.FromMinutes(30);

        public SessionManager(IBucketExtension bucketExtension)
        {
            _bucketExtension = bucketExtension;
        }

        public Task ClearSessionAsync(Node node, CancellationToken cancellationToken) 
            => _bucketExtension.DeleteAsync(GetSessionKey(node), cancellationToken);

        public async Task AddVariableAsync(Node node, string key, string value, CancellationToken cancellationToken)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            var session = await GetOrCreateSessionAsync(node, cancellationToken).ConfigureAwait(false);
            if (session.Variables == null) session.Variables = new Dictionary<string, string>();
            session.Variables[key] = value;
            await SaveSessionAsync(node, session, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetVariableAsync(Node node, string key, CancellationToken cancellationToken)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var session = await GetSessionAsync(node, cancellationToken).ConfigureAwait(false);
            if (session?.Variables == null || !session.Variables.ContainsKey(key)) return null;
            return session.Variables[key];
        }

        public async Task RemoveVariableAsync(Node node, string key, CancellationToken cancellationToken)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var session = await GetSessionAsync(node, cancellationToken).ConfigureAwait(false);
            if (session?.Variables == null || !session.Variables.ContainsKey(key)) return;
            session.Variables.Remove(key);
            await SaveSessionAsync(node, session, cancellationToken).ConfigureAwait(false);
        }

        public Task<NavigationSession> GetSessionAsync(Node node, CancellationToken cancellationToken)
            => _bucketExtension.GetAsync<NavigationSession>(GetSessionKey(node), cancellationToken);

        public Task<string> GetCultureAsync(Node node, CancellationToken cancellationToken)
            => GetVariableAsync(node, CULTURE_KEY, cancellationToken);
        

        public Task SetCultureAsync(Node node, string culture, CancellationToken cancellationToken)
            => AddVariableAsync(node, CULTURE_KEY, culture, cancellationToken);

        private async Task<NavigationSession> GetOrCreateSessionAsync(Node node, CancellationToken cancellationToken) 
            => await GetSessionAsync(node, cancellationToken) ??
                new NavigationSession()
                {
                    Creation = DateTimeOffset.UtcNow
                };

        private Task SaveSessionAsync(Node node, NavigationSession session, CancellationToken cancellationToken) 
            => _bucketExtension.SetAsync(GetSessionKey(node), session, SessionExpiration, cancellationToken);

        private static string GetSessionKey(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return $"sessions:{node.ToIdentity()}".ToLowerInvariant();
        }


    }

    public class SessionSettings
    {
        public int ExpirationInMinutes { get; set; }
    }
}