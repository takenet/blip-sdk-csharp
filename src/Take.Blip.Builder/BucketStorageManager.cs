using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Hosting;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Builder
{
    public class BucketStorageManager : IStorageManager
    {
        private const string STATE_ID_KEY = "stateId";
        private const string ACTION_ID_KEY = "actionId";

        private readonly IBucketExtension _bucketExtension;
        private readonly IConfiguration _configuration;

        public BucketStorageManager(IBucketExtension bucketExtension, IConfiguration configuration)
        {
            _bucketExtension = bucketExtension;
            _configuration = configuration;
        }

        public Task<ExecutionStatus> GetExecutionStatusAsync(string flowId, string user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetExecutionStatusAsync(string flowId, string user, ExecutionStatus executionStatus,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetStateIdAsync(string flowId, string user, CancellationToken cancellationToken) 
            => _bucketExtension.GetAsync(BucketIdHelper.GetPrivateId(flowId, user, STATE_ID_KEY), cancellationToken);

        public Task SetStateIdAsync(string flowId, string user, string stateId, CancellationToken cancellationToken) 
            => _bucketExtension.SetAsync(BucketIdHelper.GetPrivateId(flowId, user, STATE_ID_KEY), stateId, cancellationToken, 
                _configuration.SessionExpiration);

        public Task DeleteStateIdAsync(string flowId, string user, CancellationToken cancellationToken) 
            => _bucketExtension.DeleteAsync(BucketIdHelper.GetPrivateId(flowId, user, STATE_ID_KEY), cancellationToken);

        public Task<string> GetActionIdAsync(string flowId, string user, CancellationToken cancellationToken) 
            => _bucketExtension.GetAsync(BucketIdHelper.GetPrivateId(flowId, user, ACTION_ID_KEY), cancellationToken);

        public Task SetActionIdAsync(string flowId, string user, string actionId, CancellationToken cancellationToken) 
            => _bucketExtension.SetAsync(BucketIdHelper.GetPrivateId(flowId, user, ACTION_ID_KEY), actionId,
            cancellationToken, _configuration.SessionExpiration);

        public Task DeleteActionIdAsync(string flowId, string user, CancellationToken cancellationToken) 
            => _bucketExtension.DeleteAsync(BucketIdHelper.GetPrivateId(flowId, user, ACTION_ID_KEY),
            cancellationToken);
    }
}