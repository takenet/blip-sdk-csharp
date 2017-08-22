using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Client.Extensions.ArtificialIntelligence
{
    [Obsolete("Use IArtificialIntelligenceExtension instead")]
    public interface ITalkServiceExtension
    {
        Task<Analysis> AnalysisAsync(string sentence, CancellationToken cancellationToken);
    }
}
