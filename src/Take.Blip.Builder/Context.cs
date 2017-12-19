using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Take.Blip.Client.Extensions.Contacts;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder
{
    public class Context : IContext
    {
        public const string CONTACT_EXTRAS_VARIABLE_PREFIX = "extras.";


        private readonly IContextExtension _contextExtension;
        private readonly IContactExtension _contactExtension;
        private readonly ConcurrentDictionary<string, PropertyInfo> _contactPropertyCacheDictionary;


        public Context(string flowId, Identity user, IContextExtension contextExtension, IContactExtension contactExtension)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            FlowId = flowId;
            _contextExtension = contextExtension;
            _contactExtension = contactExtension;
            _contactPropertyCacheDictionary = new ConcurrentDictionary<string, PropertyInfo>();

        }

        public string FlowId { get; set; }

        public Identity User { get; }

        public Task SetVariableAsync(string name, string value, CancellationToken cancellationToken)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return _contextExtension.SetTextVariableAsync(User, name.ToLowerInvariant(), value, cancellationToken);
        }

        public async Task<string> GetVariableAsync(string name, CancellationToken cancellationToken)
        {
            var variable = VariableName.Parse(name);
            var variableValue = await GetVariableAsync(variable.Source, variable.Name, cancellationToken);

            if (string.IsNullOrWhiteSpace(variableValue) || string.IsNullOrWhiteSpace(variable.Property))
            {
                return variableValue;
            }

            return GetJsonProperty(variableValue, variable.Property);
        }

        private async Task<string> GetVariableAsync(VariableSource source, string name, CancellationToken cancellationToken)
        {
            try
            {
                switch (source)
                {
                    case VariableSource.Contact:
                        var contact = await _contactExtension.GetAsync(User, cancellationToken);
                        if (contact == null) return null;
                        return GetContactProperty(contact, name);

                    case VariableSource.Context:
                    default:
                        return await _contextExtension.GetTextVariableAsync(User, name, cancellationToken);
                }
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return null;
            }
        }

        private string GetContactProperty(Contact contact, string variableName)
        {            
            if (variableName.StartsWith(CONTACT_EXTRAS_VARIABLE_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                var extraVariableName = variableName.Remove(0, CONTACT_EXTRAS_VARIABLE_PREFIX.Length);
                if (contact.Extras != null && contact.Extras.TryGetValue(extraVariableName, out var extraVariableValue))
                {
                    return extraVariableValue;
                }
                return null;
            }

            var contactPropertyInfo = GetContactPropertyInfo(variableName.ToLowerInvariant());
            if (contactPropertyInfo != null) return contactPropertyInfo.GetValue(contact)?.ToString();

            return null;
        }

        private PropertyInfo GetContactPropertyInfo(string contactVariableName)
        {
            // Caches the properties to reduce the reflection overhead
            if (!_contactPropertyCacheDictionary.TryGetValue(contactVariableName, out var property))
            {
                property = typeof(Contact).GetProperty(contactVariableName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property != null) _contactPropertyCacheDictionary.TryAdd(contactVariableName, property);
            }

            return property;
        }


        private static string GetJsonProperty(string variableValue, string property)
        {
            try
            {
                // If there's a propertyName, attempts to parse the value as JSON and retrieve the value from it.
                var propertyNames = property.Split('.');
                JToken json = JObject.Parse(variableValue);
                foreach (var s in propertyNames)
                {
                    json = json[s];
                    if (json == null) return null;
                }

                return json.ToString(Formatting.None).Trim('"');
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private enum VariableSource
        {
            Context,
            Contact
        }

        struct VariableName
        {
            private VariableName(VariableSource source, string name, string property)
            {
                Source = source;
                Name = name;
                Property = property;
            }

            public VariableSource Source { get; }

            public string Name { get; }

            public string Property { get; }

            /// <summary>
            /// Parses a variable name string.
            /// </summary>
            /// <example>source.name@property</example>
            /// <param name="s"></param>
            /// <returns></returns>
            public static VariableName Parse(string s)
            {
                if (s == null) throw new ArgumentNullException(nameof(s));

                var variableAndProperty = s.ToLowerInvariant().Split('@');
                var property = variableAndProperty.Length > 1 ? variableAndProperty[1] : null;

                VariableSource source;
                string name;

                var dotIndex = variableAndProperty[0].IndexOf('.');
                if (dotIndex > 0)
                {
                    var sourceName = variableAndProperty[0].Substring(0, dotIndex);
                    if (!Enum.TryParse(sourceName, out source))
                    {
                        throw new ArgumentException($"Invalid source '{sourceName}'");
                    }

                    name = variableAndProperty[0].Substring(dotIndex + 1, variableAndProperty[0].Length - 1);
                }
                else
                {
                    source = VariableSource.Context;
                    name = variableAndProperty[0];
                }

                return new VariableName(source, name, property);
            }
        }
    }
}