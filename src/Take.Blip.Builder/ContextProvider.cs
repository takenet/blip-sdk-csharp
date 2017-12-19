using Lime.Protocol;
using Take.Blip.Client.Extensions.Contacts;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder
{
    public class ContextProvider : IContextProvider
    {
        private readonly IContextExtension _contextExtension;
        private readonly IContactExtension _contactExtension;

        public ContextProvider(IContextExtension contextExtension, IContactExtension contactExtension)
        {
            _contextExtension = contextExtension;
            _contactExtension = contactExtension;
        }

        public IContext GetContext(Identity user, string flowId) 
            => new Context(flowId, user, _contextExtension, _contactExtension);
    }
}