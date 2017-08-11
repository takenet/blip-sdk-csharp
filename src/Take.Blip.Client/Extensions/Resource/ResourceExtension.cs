using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Client.Extensions.Resource
{
    public class ResourceExtension : BucketExtension, IResourceExtension
    {
        public ResourceExtension(ISender sender)
            : base(sender, "resources")
        {

        }
    }
}
