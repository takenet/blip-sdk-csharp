using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Utils
{
    public class VariableReplacer : IVariableReplacer
    {
        private static readonly Regex TextVariablesRegex = new Regex(@"{{([a-zA-Z0-9\.@]+)}}", RegexOptions.Compiled);

        public async Task<string> ReplaceAsync(string value, IContext context, CancellationToken cancellationToken)
        {
            var variableValues = new Dictionary<string, string>();
            foreach (Match match in TextVariablesRegex.Matches(value))
            {
                var variableName = match.Groups[1].Value;
                if (variableValues.ContainsKey(variableName)) continue;
                var variableValue = await context.GetVariableAsync(variableName, cancellationToken);
                variableValues.Add(variableName, variableValue);
            }

            if (variableValues.Count > 0)
            {
                value = TextVariablesRegex.Replace(value, match => variableValues[match.Groups[1].Value]);
            }

            return value;
        }
    }
}
