using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions
{
    public abstract class ActionBase<TSettings> : IAction where TSettings : IValidable
    {
        protected ActionBase(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Action type cannot be null or whitespace.", nameof(type));
            Type = type;
        }

        public string Type { get; }

        public Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var validableSettings = settings.ToObject<TSettings>();
            validableSettings.Validate();

            return ExecuteAsync(context, validableSettings, cancellationToken);
        }

        public abstract Task ExecuteAsync(IContext context, TSettings settings, CancellationToken cancellationToken);
    }
}
