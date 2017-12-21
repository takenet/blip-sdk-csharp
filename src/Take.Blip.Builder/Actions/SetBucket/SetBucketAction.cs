using System;
using System.Collections.Generic;
using System.Text;
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


            throw new NotImplementedException();
        }
    }
}
