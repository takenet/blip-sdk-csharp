using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Builder.Actions.SetBucket
{
    public class SetBucketAction : ActionBase<SetBucketSettings>
    {
        private readonly IBucketExtension _bucketExtension;
        private readonly IBlipLogger _blipMonitoringLogger;

        public SetBucketAction(IBucketExtension bucketExtension, IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(SetBucket))
        {
            _bucketExtension = bucketExtension;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(IContext context, SetBucketSettings settings, CancellationToken cancellationToken)
        {
            var expiration = settings.Expiration.HasValue
                ? TimeSpan.FromSeconds(settings.Expiration.Value)
                : default(TimeSpan);

            try
            {
                await _bucketExtension.SetAsync(
                    settings.Id,
                    settings.ToDocument(),
                    expiration,
                    cancellationToken);

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "SetBucket",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["bucketId"] = settings.Id,
                        ["expiration"] = settings.Expiration,
                        ["success"] = true,
                    },
                    FlowVersion = context.Flow?.Version,
                    Channel = context.Input.Message?.From?.Domain,
                    IdMessage = context.Input.Message?.Id,
                    From = context.UserIdentity?.ToString(),
                    To = context.OwnerIdentity?.ToString(),
                    OriginalFrom = context.Input.Message?.From,
                    OriginalTo = context.Input.Message?.To,
                });
            }
            catch (Exception ex)
            {
                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "SetBucket",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["bucketId"] = settings.Id,
                        ["expiration"] = settings.Expiration,
                        ["success"] = false,
                        ["error"] = ex.ToString(),
                    },
                    FlowVersion = context.Flow?.Version,
                    Channel = context.Input.Message?.From?.Domain,
                    IdMessage = context.Input.Message?.Id,
                    From = context.UserIdentity?.ToString(),
                    To = context.OwnerIdentity?.ToString(),
                    OriginalFrom = context.Input.Message?.From,
                    OriginalTo = context.Input.Message?.To,
                });
                throw;
            }
        }
    }
}
