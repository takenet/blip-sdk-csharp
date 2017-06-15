using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Client.Extensions.ArtificialIntelligence
{
    public class TalkServiceExtension : ExtensionBase, ITalkServiceExtension
    {
        private static readonly Node TalkServiceAddress = Node.Parse($"postmaster@talkservice.{Constants.DEFAULT_DOMAIN}");

        public TalkServiceExtension(ISender sender)
            : base(sender)
        {

        }

        public Task<Analysis> AnalysisAsync(string sentence, CancellationToken cancellationToken)
        {
            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = TalkServiceAddress,
                Method = CommandMethod.Get,
                Uri = new LimeUri($"/analysis?sentence={Uri.EscapeDataString(sentence)}")
            };

            return ProcessCommandAsync<Analysis>(requestCommand, cancellationToken);
        }
    }
}
