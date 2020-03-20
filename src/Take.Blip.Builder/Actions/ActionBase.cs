using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions
{
    public abstract class ActionBase<TSettings> : IAction
    {
        protected ActionBase(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Action type cannot be null or whitespace.", nameof(type));
            ActionName = type;
        }

        public Type SettingsType => typeof(TSettings);

        public string ActionName { get; }

        public Task ExecuteAsync(IContext context, object settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var typedSettings = (TSettings)settings;

            if (typedSettings is IValidable validableSettings)
            {
                validableSettings.Validate();
            }

            try
            {
                return ExecuteAsync(context, typedSettings, cancellationToken);
            }
            finally
            {
                if (typedSettings is IDisposable disposableSettings)
                {
                    disposableSettings.Dispose();
                }
            }
        }

        public abstract Task ExecuteAsync(IContext context, TSettings settings, CancellationToken cancellationToken);
    }
}
