using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Builder
{
    public class BuilderExtension : ExtensionBase, IBuilderExtension
    {
        private static readonly Node BuilderAddress = Node.Parse($"postmaster@builder.{Constants.DEFAULT_DOMAIN}");
        public BuilderExtension(ISender sender) : base(sender)
        {
        }

        public async Task<Document> GetFunctionOnBlipFunctionAsync(string functionId, CancellationToken cancellationToken) 
        {
            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = BuilderAddress,
                Method = CommandMethod.Get,
                Uri = new LimeUri($"/functions/{functionId}"),
            };

              return await ProcessCommandAsync<Document>(requestCommand, cancellationToken);
        }

    }
}
