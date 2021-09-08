using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Serilog;

namespace Take.Blip.Builder.Variables
{
    /// <summary>
    /// Defines a base provider that can retrieve variables for a specific user.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UserVariableProviderBase<T> : IVariableProvider
    {
        private readonly ConcurrentDictionary<string, PropertyInfo> _propertyCacheDictionary;
        private readonly string _inputContextKey;
        private readonly ILogger _logger;

        protected UserVariableProviderBase(VariableSource source, string inputContextKey, ILogger logger)
        {
            Source = source;
            _inputContextKey = inputContextKey;
            _logger = logger;
            _propertyCacheDictionary = new ConcurrentDictionary<string, PropertyInfo>();
        }

        public VariableSource Source { get; }

        public async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
            T item;
            try
            {
                item = context.GetValue<T>(_inputContextKey);
                if (item == null)
                {
                    item = await GetAsync(context.UserIdentity, cancellationToken);
                    context.SetValue(_inputContextKey, item);
                }

                if (item == null) return null;
                
                return GetProperty(item, name);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                context.RemoveValue(_inputContextKey);
                _logger.Warning(ex, "An exception occurred while obtaining variable {Name} from {InputContextKey}", name, _inputContextKey);
                return null;
            }
        }

        protected abstract Task<T> GetAsync(Identity userIdentity, CancellationToken cancellationToken);

        protected virtual string GetProperty(T item, string propertyName)
        {
            var itemPropertyInfo = GetPropertyInfo(propertyName.ToLowerInvariant());
            if (itemPropertyInfo != null) return itemPropertyInfo.GetValue(item)?.ToString();
            return null;
        }

        private PropertyInfo GetPropertyInfo(string propertyName)
        {
            // Caches the properties to reduce the reflection overhead
            return _propertyCacheDictionary.GetOrAdd(
                propertyName,
                p => typeof(T).GetProperty(propertyName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance));
        }
    }
}