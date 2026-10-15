using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.PaymentBlock.Tests.Comparison
{
    /// <summary>
    /// Normalizes the JSON captured from an engine run so it can be compared with the approved references.
    /// Normalization is deterministic: it sorts the properties of each object (ordinal), keeps the array order
    /// and replaces with fixed markers only what is in <see cref="VOLATILE_RULES"/>. Everything else is compared as is.
    /// </summary>
    public static class JsonNormalizer
    {
        private const string SECRET_MARKER = "<segredo>";
        private const int MIN_RUN_SECRET_LENGTH = 8;
        private static readonly TimeSpan REGEX_TIMEOUT = TimeSpan.FromSeconds(1);

        private static readonly JsonLoadSettings LOAD_SETTINGS = new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Ignore,
            DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
            LineInfoHandling = LineInfoHandling.Ignore,
        };

        /// <summary>
        /// Closed list of what is volatile, in the order it is applied. It is the single source: nothing outside it is ignored in the comparison.
        /// <list type="number">
        /// <item><description><c>&lt;segredo&gt;</c>: whole value of the properties <c>authorization</c>, <c>accessKey</c>, <c>accessToken</c>, <c>apiKey</c>, <c>botToken</c>, <c>password</c>, <c>secret</c> and <c>token</c> (case-insensitive) and, inside any text, the secrets and tokens generated in the run that the caller passes in <c>runSecrets</c>.</description></item>
        /// <item><description><c>&lt;envelope-id&gt;</c>: envelope <c>id</c> (object with <c>to</c>, <c>from</c> or <c>method</c> and <c>uri</c>), which the engine generates on every send.</description></item>
        /// <item><description><c>&lt;id-motor&gt;</c>: engine id metadata <c>#inReplyToId</c>, <c>#uniqueId</c> and <c>#message.uniqueId</c>.</description></item>
        /// <item><description><c>&lt;guid&gt;</c>: GUID inside text, except the zero one (<c>00000000-0000-0000-0000-000000000000</c>).</description></item>
        /// <item><description><c>&lt;data-hora&gt;</c>: ISO 8601 date/time inside text (<c>YYYY-MM-DDTHH:MM</c>, with optional seconds, fraction and offset) and date-typed value.</description></item>
        /// <item><description><c>&lt;epoch-ms&gt;</c>: Unix time in milliseconds (13 digits, from 2017 to 2042) right after the <c>REF</c> prefix, the reference id the block script generates with <c>"REF" + Date.now()</c> when the configuration has none. The prefix stays in the text (<c>REF&lt;epoch-ms&gt;</c>); a 13-digit number without that prefix is compared as is.</description></item>
        /// <item><description><c>&lt;epoch-s&gt;</c>: whole value of the <c>timestamp</c> property, the Unix time in seconds (<c>Math.floor(Date.now() / 1000)</c>) that the feedback script writes to <c>payment.timestamp</c> of the status message.</description></item>
        /// </list>
        /// Property names go through the same text rules; if two names turn into the same marker, they get the suffix <c>#2</c>, <c>#3</c>... in the order of the normalized value.
        /// </summary>
        public static readonly IReadOnlyList<VolatileRule> VOLATILE_RULES = new[]
        {
            VolatileRule.ForProperties(SECRET_MARKER, StringComparer.OrdinalIgnoreCase, false, "authorization", "accessKey", "accessToken", "apiKey", "botToken", "password", "secret", "token"),
            VolatileRule.ForProperties("<envelope-id>", StringComparer.Ordinal, true, "id"),
            VolatileRule.ForProperties("<id-motor>", StringComparer.Ordinal, false, "#inReplyToId", "#uniqueId", "#message.uniqueId"),
            VolatileRule.ForText("<guid>", new Regex("(?!0{8}-0{4}-0{4}-0{4}-0{12})[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", RegexOptions.CultureInvariant, REGEX_TIMEOUT)),
            VolatileRule.ForText("<data-hora>", new Regex("(?<![0-9])[0-9]{4}-[0-9]{2}-[0-9]{2}[T ][0-9]{2}:[0-9]{2}(?::[0-9]{2}(?:[.,][0-9]+)?)?(?:Z|[+-][0-9]{2}(?::?[0-9]{2})?)?", RegexOptions.CultureInvariant, REGEX_TIMEOUT)),
            VolatileRule.ForText("<epoch-ms>", new Regex("(?<=\\bREF)(?:1[5-9]|2[0-2])[0-9]{11}(?![0-9])", RegexOptions.CultureInvariant, REGEX_TIMEOUT)),
            VolatileRule.ForProperties("<epoch-s>", StringComparer.Ordinal, false, "timestamp"),
        };

        private static readonly string DATE_TIME_MARKER = VOLATILE_RULES[4].Marker;

        /// <summary>
        /// Returns a normalized copy of <paramref name="value"/>, without changing the original.
        /// </summary>
        /// <param name="value">JSON captured from the run.</param>
        /// <param name="runSecrets">Secrets and tokens generated in this run (at least 8 characters each), replaced with <c>&lt;segredo&gt;</c> wherever they appear.</param>
        public static JToken Normalize(JToken value, IEnumerable<string> runSecrets = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return NormalizeToken(value, PrepareSecrets(runSecrets));
        }

        /// <summary>
        /// Reads JSON without converting dates or losing decimal places, rejecting duplicate properties and content past the end.
        /// </summary>
        public static JToken Parse(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            using (var reader = new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None, FloatParseHandling = FloatParseHandling.Decimal })
            {
                var token = JToken.ReadFrom(reader, LOAD_SETTINGS);
                while (reader.Read())
                {
                    if (reader.TokenType != JsonToken.Comment)
                    {
                        throw new JsonReaderException("Conteúdo além do fim do JSON.");
                    }
                }

                return token;
            }
        }

        /// <summary>
        /// Canonical text of the normalized JSON: indented with 2 spaces, <c>\n</c> line breaks and a trailing <c>\n</c>.
        /// </summary>
        public static string ToCanonicalText(JToken normalized)
        {
            if (normalized == null)
            {
                throw new ArgumentNullException(nameof(normalized));
            }

            var builder = new StringBuilder();
            using (var stringWriter = new StringWriter(builder, CultureInfo.InvariantCulture) { NewLine = "\n" })
            using (var writer = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented, Indentation = 2, IndentChar = ' ' })
            {
                normalized.WriteTo(writer);
            }

            builder.Append('\n');
            return builder.ToString();
        }

        private static string[] PrepareSecrets(IEnumerable<string> runSecrets)
        {
            if (runSecrets == null)
            {
                return Array.Empty<string>();
            }

            var secrets = new List<string>();
            foreach (var secret in runSecrets)
            {
                if (secret == null || secret.Length < MIN_RUN_SECRET_LENGTH)
                {
                    throw new ArgumentException("Todo segredo de execução precisa ter ao menos 8 caracteres.", nameof(runSecrets));
                }

                if (!secrets.Contains(secret))
                {
                    secrets.Add(secret);
                }
            }

            secrets.Sort(CompareSecrets);
            return secrets.ToArray();
        }

        private static int CompareSecrets(string left, string right)
        {
            var byLength = right.Length.CompareTo(left.Length);
            return byLength != 0 ? byLength : string.CompareOrdinal(left, right);
        }

        private static JToken NormalizeToken(JToken token, string[] secrets)
        {
            switch (token)
            {
                case JObject jsonObject:
                    return NormalizeObject(jsonObject, secrets);
                case JArray array:
                    var normalizedArray = new JArray();
                    foreach (var item in array)
                    {
                        normalizedArray.Add(NormalizeToken(item, secrets));
                    }

                    return normalizedArray;
                case JValue value:
                    return NormalizeValue(value, secrets);
                default:
                    return token.DeepClone();
            }
        }

        private static JObject NormalizeObject(JObject source, string[] secrets)
        {
            var envelope = source.ContainsKey("to") || source.ContainsKey("from") || (source.ContainsKey("method") && source.ContainsKey("uri"));
            var entries = new List<KeyValuePair<string, JToken>>(source.Count);
            foreach (var property in source.Properties())
            {
                var rule = FindPropertyRule(property.Name, envelope);
                var value = rule == null || property.Value.Type == JTokenType.Null
                    ? NormalizeToken(property.Value, secrets)
                    : new JValue(rule.Marker);
                entries.Add(new KeyValuePair<string, JToken>(ReplaceText(property.Name, secrets), value));
            }

            entries.Sort(CompareEntries);
            var names = new HashSet<string>(StringComparer.Ordinal);
            var unique = new List<KeyValuePair<string, JToken>>(entries.Count);
            foreach (var entry in entries)
            {
                var name = entry.Key;
                for (var suffix = 2; !names.Add(name); suffix++)
                {
                    name = entry.Key + "#" + suffix.ToString(CultureInfo.InvariantCulture);
                }

                unique.Add(new KeyValuePair<string, JToken>(name, entry.Value));
            }

            unique.Sort(CompareEntries);
            var normalized = new JObject();
            foreach (var entry in unique)
            {
                normalized.Add(entry.Key, entry.Value);
            }

            return normalized;
        }

        private static int CompareEntries(KeyValuePair<string, JToken> left, KeyValuePair<string, JToken> right)
        {
            var byName = string.CompareOrdinal(left.Key, right.Key);
            return byName != 0 ? byName : string.CompareOrdinal(left.Value.ToString(Formatting.None), right.Value.ToString(Formatting.None));
        }

        private static VolatileRule FindPropertyRule(string name, bool envelope)
        {
            foreach (var rule in VOLATILE_RULES)
            {
                if (rule.MatchesProperty(name, envelope))
                {
                    return rule;
                }
            }

            return null;
        }

        private static JToken NormalizeValue(JValue value, string[] secrets)
        {
            switch (value.Type)
            {
                case JTokenType.String:
                    return new JValue(ReplaceText((string)value.Value, secrets));
                case JTokenType.Date:
                    return new JValue(DATE_TIME_MARKER);
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                    return new JValue(ReplaceText(Convert.ToString(value.Value, CultureInfo.InvariantCulture), secrets));
                case JTokenType.Integer:
                    var text = Convert.ToString(value.Value, CultureInfo.InvariantCulture);
                    var replaced = ReplaceText(text, secrets);
                    return string.Equals(text, replaced, StringComparison.Ordinal) ? value.DeepClone() : new JValue(replaced);
                default:
                    return value.DeepClone();
            }
        }

        private static string ReplaceText(string text, string[] secrets)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            foreach (var secret in secrets)
            {
                text = text.Replace(secret, SECRET_MARKER, StringComparison.Ordinal);
            }

            foreach (var rule in VOLATILE_RULES)
            {
                if (rule.TextPattern != null)
                {
                    text = rule.TextPattern.Replace(text, rule.Marker);
                }
            }

            return text;
        }

        /// <summary>
        /// Volatility rule: replaces with <see cref="Marker"/> the whole value of the properties in <see cref="PropertyNames"/>
        /// or each piece of text that matches <see cref="TextPattern"/>.
        /// </summary>
        public sealed class VolatileRule
        {
            private readonly HashSet<string> _propertyNames;

            private VolatileRule(string marker, HashSet<string> propertyNames, bool envelopeOnly, Regex textPattern)
            {
                Marker = marker;
                _propertyNames = propertyNames;
                EnvelopeOnly = envelopeOnly;
                TextPattern = textPattern;
            }

            /// <summary>Fixed marker that takes the place of the volatile value.</summary>
            public string Marker { get; }

            /// <summary>Property names whose whole value is volatile; empty when the rule is a text pattern.</summary>
            public IReadOnlyCollection<string> PropertyNames => _propertyNames;

            /// <summary>When true, the rule only applies to properties of an envelope object.</summary>
            public bool EnvelopeOnly { get; }

            /// <summary>Pattern searched inside texts; null when the rule is by property name.</summary>
            public Regex TextPattern { get; }

            internal static VolatileRule ForProperties(string marker, StringComparer comparer, bool envelopeOnly, params string[] names)
            {
                return new VolatileRule(marker, new HashSet<string>(names, comparer), envelopeOnly, null);
            }

            internal static VolatileRule ForText(string marker, Regex pattern)
            {
                return new VolatileRule(marker, new HashSet<string>(StringComparer.Ordinal), false, pattern);
            }

            internal bool MatchesProperty(string name, bool envelope)
            {
                return _propertyNames.Count > 0 && (envelope || !EnvelopeOnly) && _propertyNames.Contains(name);
            }
        }
    }
}
