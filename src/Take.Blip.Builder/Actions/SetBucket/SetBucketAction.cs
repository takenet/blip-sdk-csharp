using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Builder.Actions.SetBucket
{
    public class SetBucketAction : IAction
    {
        private readonly IBucketExtension _bucketExtension;

        public SetBucketAction(IBucketExtension bucketExtension)
        {
            _bucketExtension = bucketExtension;
        }

        public string Type => nameof(SetBucket);

        public Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var setBucketActionSettings = settings.ToObject<SetBucketActionSettings>();
            if (setBucketActionSettings.Id == null)
            {
                throw new ArgumentException($"The '{nameof(SetBucketActionSettings.Id)}' settings value is required for '{nameof(SetBucket)}' action");
            }
            if (setBucketActionSettings.Type == null)
            {
                throw new ArgumentException($"The '{nameof(SetBucketActionSettings.Type)}' settings value is required for '{nameof(SetBucket)}' action");
            }

            var expiration = setBucketActionSettings.Expiration.HasValue
                ? TimeSpan.FromMilliseconds(setBucketActionSettings.Expiration.Value)
                : default(TimeSpan);

            return _bucketExtension.SetAsync(
                setBucketActionSettings.Id,
                setBucketActionSettings.ToDocument(),
                expiration,
                cancellationToken);
        }
    }
}
