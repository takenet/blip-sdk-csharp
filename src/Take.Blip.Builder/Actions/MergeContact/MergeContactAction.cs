using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Newtonsoft.Json.Linq;
using Serilog;
using Blip.Ai.Bot.Monitoring.Logging.Interface;
using Blip.Ai.Bot.Monitoring.Logging.Services;
using Blip.Ai.Bot.Monitoring.Logging.Models;
using Take.Blip.Client.Extensions.Contacts;

namespace Take.Blip.Builder.Actions.MergeContact
{
    public class MergeContactAction : IAction
    {
        private readonly IContactExtension _contactExtension;
        private readonly ILogger _logger;
        private readonly IBlipLogger _blipMonitoringLogger;

        public MergeContactAction(IContactExtension contactExtension, ILogger logger, IBlipLogger? blipMonitoringLogger = null)
        {
            _contactExtension = contactExtension;
            _logger = logger;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public string Type => nameof(MergeContact);

        public string[]? OutputVariables => null;

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            try
            {
                var contact = settings.ToObject<Contact>(LimeSerializerContainer.Serializer);
                contact.Identity = contact.Identity;

                _logger.Information("Trying to merge contact values ({Settings}) for UserIdentity {UserIdentity}",
                   settings,
                   context.UserIdentity);

                await _contactExtension.MergeAsync(context.UserIdentity, contact, cancellationToken);
                context.RemoveContact();

                this.LogExecution(_blipMonitoringLogger, context, new JObject
                {
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }, settings);
            }
            catch (Exception ex)
            {
                this.LogError(_blipMonitoringLogger, context, new JObject
                {
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }, ex);
                throw;
            }
        }
    }
}
