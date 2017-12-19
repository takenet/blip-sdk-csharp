using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Take.Blip.Client.Extensions.Contacts;

namespace Take.Blip.Builder.Actions.SetContactProperty
{
    public class SetContactPropertyAction : IAction
    {
        private readonly IContactExtension _contactExtension;

        public SetContactPropertyAction(IContactExtension contactExtension)
        {
            _contactExtension = contactExtension;
        }

        public string Type => nameof(SetContactProperty);

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
