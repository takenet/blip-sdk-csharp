using System.Threading;
using System.Threading.Tasks;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Client.Extensions.ArtificialIntelligence
{
    public interface ITalkServiceExtension
    {
        Task<Analysis> AnalysisAsync(string sentence, CancellationToken cancellationToken);
    }
}
