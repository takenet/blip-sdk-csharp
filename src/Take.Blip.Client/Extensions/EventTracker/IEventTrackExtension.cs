using Lime.Messaging.Resources;
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

        Task AddAsync(string category, string action, string label = null, Message message = null, Contact contact = null, string contactExternalId = null, decimal? value = null, IDictionary<string, string> extras = null, CancellationToken cancellationToken = new CancellationToken());

        Task AddAsync(string category, string action, string label = null, string messageId = null, string contactIdentity = null, string contactSource = null, string contactGroup = null, string contactExternalId = null, decimal? value = null, IDictionary<string, string> extras = null, CancellationToken cancellationToken = new CancellationToken());

        Task<DocumentCollection> GetCategoriesAsync(int take = 20, CancellationToken cancellationToken = new CancellationToken());

        Task<DocumentCollection> GetCategoryActionsCounterAsync(DateTimeOffset startDate, DateTimeOffset endDate, string category, int take = 20, CancellationToken cancellationToken = new CancellationToken());

        Task<DocumentCollection> GetAllAsync(DateTimeOffset startDate, DateTimeOffset endDate, string category, string action, int skip = 0, int take = 20, CancellationToken cancellationToken = new CancellationToken());
    }
}
