using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Builder
{
    public static class BucketExtensionExtensions
    {
        public static Task SetAsync(this IBucketExtension bucketExtension, string id, string value, CancellationToken cancellationToken, TimeSpan expiration = default(TimeSpan))
            => bucketExtension.SetAsync(id, new PlainText { Text = value }, expiration, cancellationToken);

        public static async Task<string> GetAsync(this IBucketExtension bucketExtension, string id, CancellationToken cancellationToken)
        {
            var plainText = await bucketExtension.GetAsync<PlainText>(id, cancellationToken);
            return plainText?.Text;
        }
    }
}