using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Hosting;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Builder
{
    public class StorageManager : IStorageManager
    {
        private const string STATE_ID_KEY = "stateId";
        private const string ACTION_ID_KEY = "actionId";

        private readonly IBucketExtension _bucketExtension;
        private readonly IConfiguration _configuration;

        public StorageManager(IBucketExtension bucketExtension, IConfiguration configuration)
        {
            _bucketExtension = bucketExtension;
            _configuration = configuration;
        }

        public Task<string> GetStateIdAsync(string flowId, Identity user, CancellationToken cancellationToken) 
            => _bucketExtension.GetAsync(BucketIdHelper.GetPrivateId(flowId, user, STATE_ID_KEY), cancellationToken);

        public Task SetStateIdAsync(string flowId, Identity user, string stateId, CancellationToken cancellationToken) 
            => _bucketExtension.SetAsync(BucketIdHelper.GetPrivateId(flowId, user, STATE_ID_KEY), stateId, cancellationToken, 
                _configuration.SessionExpiration);

        public Task DeleteStateIdAsync(string flowId, Identity user, CancellationToken cancellationToken) 
            => _bucketExtension.DeleteAsync(BucketIdHelper.GetPrivateId(flowId, user, STATE_ID_KEY), cancellationToken);

        public Task<string> GetActionIdAsync(string flowId, Identity user, CancellationToken cancellationToken) 
            => _bucketExtension.GetAsync(BucketIdHelper.GetPrivateId(flowId, user, ACTION_ID_KEY), cancellationToken);
    }
}