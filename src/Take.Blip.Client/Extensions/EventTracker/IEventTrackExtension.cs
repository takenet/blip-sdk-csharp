using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Extensions.EventTracker
{
    public interface IEventTrackExtension
    {
        Task AddAsync(string category, string action, IDictionary<string, string> extras = null, CancellationToken cancellationToken = new CancellationToken(), Identity identity = null);

        Task<DocumentCollection> GetCategoriesAsync(int take = 20, CancellationToken cancellationToken = new CancellationToken());

        Task<DocumentCollection> GetCategoryActionsCounterAsync(DateTimeOffset startDate, DateTimeOffset endDate, string category, int take = 20, CancellationToken cancellationToken = new CancellationToken());

        Task<DocumentCollection> GetAllAsync(DateTimeOffset startDate, DateTimeOffset endDate, string category, string action, int skip = 0, int take = 20, CancellationToken cancellationToken = new CancellationToken());
    }
}
