using System.Collections.Generic;
using System.Text;
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
                variableValues.Add(variableName, EscapeString(variableValue));
            }
            
            if (variableValues.Count > 0)
            {
                value = TextVariablesRegex.Replace(value, match => variableValues[match.Groups[1].Value]);
            }

            return value;
        }

        /// <summary>
        /// https://stackoverflow.com/a/39527149/704742
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private static string EscapeString(string src)
        {
            if (string.IsNullOrWhiteSpace(src)) return src;

            var sb = new StringBuilder();

            int start = 0;
            for (int i = 0; i < src.Length; i++)
                if (NeedEscape(src, i))
                {
                    sb.Append(src, start, i - start);
                    switch (src[i])
                    {
                        case '\b': sb.Append("\\b"); break;
                        case '\f': sb.Append("\\f"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        case '\"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '/': sb.Append("\\/"); break;
                        default:
                            sb.Append("\\u");
                            sb.Append(((int)src[i]).ToString("x04"));
                            break;
                    }
                    start = i + 1;
                }
            sb.Append(src, start, src.Length - start);
            return sb.ToString();
        }

        private static bool NeedEscape(string src, int i)
        {
            char c = src[i];
            return c < 32 || c == '"' || c == '\\'
                   // Broken lead surrogate
                   || (c >= '\uD800' && c <= '\uDBFF' &&
                       (i == src.Length - 1 || src[i + 1] < '\uDC00' || src[i + 1] > '\uDFFF'))
                   // Broken tail surrogate
                   || (c >= '\uDC00' && c <= '\uDFFF' &&
                       (i == 0 || src[i - 1] < '\uD800' || src[i - 1] > '\uDBFF'))
                   // To produce valid JavaScript
                   || c == '\u2028' || c == '\u2029'
                   // Escape "</" for <script> tags
                   || (c == '/' && i > 0 && src[i - 1] == '<');
        }
    }
}
