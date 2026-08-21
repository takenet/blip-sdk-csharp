using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Blip.Ai.Bot.Monitoring.Logging.Interface;
using Blip.Ai.Bot.Monitoring.Logging.Models;
using Blip.Ai.Bot.Monitoring.Logging.Services;
using Newtonsoft.Json.Linq;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Builder.Actions.SetBucket
{
    public class SetBucketAction : ActionBase<SetBucketSettings>
    {
        private readonly IBucketExtension _bucketExtension;
        private readonly IBlipLogger _blipMonitoringLogger;

        public SetBucketAction(
            IBucketExtension bucketExtension,
            IBlipLogger? blipMonitoringLogger = null
        )
            : base(nameof(SetBucket))
        {
            _bucketExtension = bucketExtension;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(
            IContext context,
            SetBucketSettings settings,
            CancellationToken cancellationToken
        )
        {
            var sw = Stopwatch.StartNew();
            var expiration = settings.Expiration.HasValue
                ? TimeSpan.FromSeconds(settings.Expiration.Value)
                : default(TimeSpan);

            try
            {
                await _bucketExtension.SetAsync(
                    settings.Id,
                    settings.ToDocument(),
                    expiration,
                    cancellationToken
                );

                this.LogExecution(
                    _blipMonitoringLogger,
                    context,
                    new JObject
                    {
                        ["bucketId"] = settings.Id,
                        ["expiration"] = settings.Expiration,
                        ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                    }
                );
            }
            catch (Exception ex)
            {
                this.LogError(
                    _blipMonitoringLogger,
                    context,
                    new JObject
                    {
                        ["bucketId"] = settings.Id,
                        ["expiration"] = settings.Expiration,
                        ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                    },
                    ex
                );
                throw;
            }
        }
    }
}
