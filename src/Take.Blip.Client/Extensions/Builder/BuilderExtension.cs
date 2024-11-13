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

        public async Task<Document> GetFunctionOnBlipFunctionAsync(string nameFunction, CancellationToken cancellationToken) 
        {
            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = BuilderAddress,
                Method = CommandMethod.Get,
                Uri = new LimeUri($"/contexts/ec1b9af1-7e28-49f6-bea0-d1bc8951acf7.testecassandra2@0mn.io/teste"),
            };

            return await ProcessCommandAsync<Document>(requestCommand, cancellationToken);
        }
            
    }
}
