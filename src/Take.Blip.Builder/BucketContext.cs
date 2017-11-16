using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Builder
{
    public class BucketContext : IContext
    {
        private readonly IBucketExtension _bucketExtension;

        public BucketContext(IBucketExtension bucketExtension, string user)
        {
            _bucketExtension = bucketExtension;
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public string FlowId { get; set; }

        public string User { get; }

        public Task SetVariableAsync(string name, string value, CancellationToken cancellationToken) 
            => _bucketExtension.SetAsync(BucketIdHelper.GetId(FlowId, User, name), value, cancellationToken);

        public Task<string> GetVariableAsync(string name, CancellationToken cancellationToken) 
            => _bucketExtension.GetAsync(BucketIdHelper.GetId(FlowId, User, name), cancellationToken);
    }
}