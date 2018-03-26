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

        /// <summary>
        /// Update messages events on supported integration application (Now only Chatbase supports this feature)
        /// </summary>
        /// <param name="extras"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        Task UpdateMessageTrackAsync(string messageId, IDictionary<string, string> extras = null, CancellationToken cancellationToken = default(CancellationToken), Identity identity = null);

        Task<DocumentCollection> GetCategoriesAsync(int take = 20, CancellationToken cancellationToken = new CancellationToken());

        Task<DocumentCollection> GetCategoryActionsCounterAsync(DateTimeOffset startDate, DateTimeOffset endDate, string category, int take = 20, CancellationToken cancellationToken = new CancellationToken());

        Task<DocumentCollection> GetAllAsync(DateTimeOffset startDate, DateTimeOffset endDate, string category, string action, int skip = 0, int take = 20, CancellationToken cancellationToken = new CancellationToken());
    }
}
