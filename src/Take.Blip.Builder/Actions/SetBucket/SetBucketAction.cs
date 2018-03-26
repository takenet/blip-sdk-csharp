using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Builder.Actions.SetBucket
{
    public class SetBucketAction : ActionBase<SetBucketSettings>
    {
        private readonly IBucketExtension _bucketExtension;

        public SetBucketAction(IBucketExtension bucketExtension)
            : base(nameof(SetBucket))
        {
            _bucketExtension = bucketExtension;
        }

        public override Task ExecuteAsync(IContext context, SetBucketSettings settings, CancellationToken cancellationToken)
        {
            var expiration = settings.Expiration.HasValue
                ? TimeSpan.FromSeconds(settings.Expiration.Value)
                : default(TimeSpan);

            return _bucketExtension.SetAsync(
                settings.Id,
                settings.ToDocument(),
                expiration,
                cancellationToken);
        }
    }
}
