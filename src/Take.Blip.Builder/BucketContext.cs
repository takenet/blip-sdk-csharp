using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Newtonsoft.Json;
using Take.Blip.Builder.Utils;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Builder
{
    public class BucketContext : IContext
    {
        private readonly IBucketExtension _bucketExtension;
        private const string KEY_PREFIX = "builder";
        private const string PRIVATE_KEY_PREFIX = "!!";
        private const string STATE_ID_KEY = "stateId";
        private const string ACTION_ID_KEY = "actionId";

        public BucketContext(IBucketExtension bucketExtension, string user)
        {
            _bucketExtension = bucketExtension;
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public string User { get; }

        public async Task SetVariableAsync(string name, string value, CancellationToken cancellationToken)
        {
            await _bucketExtension.SetAsync(GetKey(name), new PlainText { Text = value }, cancellationToken: cancellationToken);
        }

        public async Task<string> GetVariableAsync(string name, CancellationToken cancellationToken)
        {
            var plainText = await _bucketExtension.GetAsync<PlainText>(GetKey(name), cancellationToken);        
            return plainText?.Text;
        }

        public Task<string> GetStateIdAsync(CancellationToken cancellationToken)
        {
            return GetVariableAsync(GetPrivateKey(STATE_ID_KEY), cancellationToken);
        }

        public Task SetStateIdAsync(string stateId, CancellationToken cancellationToken)
        {
            return SetVariableAsync(GetPrivateKey(STATE_ID_KEY), stateId, cancellationToken);
        }

        public Task<string> GetActionIdAsync(CancellationToken cancellationToken)
        {
            return GetVariableAsync(GetPrivateKey(ACTION_ID_KEY), cancellationToken);
        }

        public Task SetActionIdAsync(string actionId, CancellationToken cancellationToken)
        {
            return SetVariableAsync(GetPrivateKey(ACTION_ID_KEY), actionId, cancellationToken);
        }

        private string GetKey(string name)
        {
            return $"{KEY_PREFIX}:{User}:{name}";
        }
        private string GetPrivateKey(string name)
        {
            return GetKey($"{PRIVATE_KEY_PREFIX}{name}");
        }
    }
}