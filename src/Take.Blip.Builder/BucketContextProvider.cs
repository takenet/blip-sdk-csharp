using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Builder
{
    public class BucketContextProvider : IContextProvider
    {
        private readonly IBucketExtension _bucketExtension;

        public BucketContextProvider(IBucketExtension bucketExtension)
        {
            _bucketExtension = bucketExtension;
        }

        public Task<IContext> GetContextAsync(string flowId, string user, CancellationToken cancellationToken)
        {
            return Task.FromResult<IContext>(new BucketContext(_bucketExtension, user));
        }
    }
}