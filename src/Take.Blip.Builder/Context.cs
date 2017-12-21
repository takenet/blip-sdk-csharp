using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Variables;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder
{
    public class Context : IContext
    {
        private readonly IContextExtension _contextExtension;
        private readonly IDictionary<VariableSource, IVariableProvider> _variableProviderDictionary;

        public Context(
            string flowId, 
            Identity user, 
            IContextExtension contextExtension,
            IEnumerable<IVariableProvider> variableProviders)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            FlowId = flowId;
            _contextExtension = contextExtension;
            _variableProviderDictionary = variableProviders.ToDictionary(v => v.Source, v => v);
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

            if (!_variableProviderDictionary.TryGetValue(variable.Source, out var provider))
            {
                throw new ArgumentException($"There's no provider for variable source '{variable.Source}'");
            }

            var variableValue = await provider.GetVariableAsync(variable.Name, User, cancellationToken);

            if (string.IsNullOrWhiteSpace(variableValue) || string.IsNullOrWhiteSpace(variable.Property))
            {
                return variableValue;
            }

            return GetJsonProperty(variableValue, variable.Property);
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

        struct VariableName
        {
            private static readonly Regex VariableNameRegex = new Regex("^(?<sourceOrName>[\\w\\d]+)(\\.(?<name>[\\w\\d\\.]+))?(@(?<property>[\\w\\d\\.]+))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

                var match = VariableNameRegex.Match(s);

                if (!match.Success) throw new ArgumentException($"Invalid variable name '{s}'", nameof(s));

                VariableSource source;
                string name;

                var sourceOrName = match.Groups["sourceOrName"].Value;
                var nameGroup = match.Groups["name"];
                if (nameGroup.Success)
                {
                    if (!Enum.TryParse(sourceOrName, true, out source))
                    {
                        throw new ArgumentException($"Invalid source '{sourceOrName}'");
                    }
                    name = nameGroup.Value;
                }
                else
                {
                    source = VariableSource.Context;
                    name = sourceOrName;
                }

                var propertyGroup = match.Groups["property"];
                var property = propertyGroup.Success ? propertyGroup.Value : null;

                return new VariableName(source, name, property);
            }
        }
    }
}