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
    /// Normaliza o JSON capturado de uma execução do motor para compará-lo com as referências aprovadas.
    /// A normalização é determinística: ordena as propriedades de cada objeto (ordinal), mantém a ordem dos arrays
    /// e troca por marcadores fixos só o que está em <see cref="VOLATILE_RULES"/>. Todo o resto é comparado como veio.
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
        /// Lista fechada do que é volátil, na ordem em que é aplicada. É a fonte única: nada fora dela é ignorado na comparação.
        /// <list type="number">
        /// <item><description><c>&lt;segredo&gt;</c>: valor inteiro das propriedades <c>authorization</c>, <c>accessKey</c>, <c>accessToken</c>, <c>apiKey</c>, <c>botToken</c>, <c>password</c>, <c>secret</c> e <c>token</c> (sem diferenciar maiúsculas) e, dentro de qualquer texto, os segredos e tokens gerados na execução que o chamador informa em <c>runSecrets</c>.</description></item>
        /// <item><description><c>&lt;envelope-id&gt;</c>: <c>id</c> de envelope (objeto com <c>to</c>, <c>from</c> ou <c>method</c> e <c>uri</c>), que o motor gera a cada envio.</description></item>
        /// <item><description><c>&lt;id-motor&gt;</c>: metadados de id do motor <c>#inReplyToId</c>, <c>#uniqueId</c> e <c>#message.uniqueId</c>.</description></item>
        /// <item><description><c>&lt;guid&gt;</c>: GUID dentro de texto, exceto o zero (<c>00000000-0000-0000-0000-000000000000</c>).</description></item>
        /// <item><description><c>&lt;data-hora&gt;</c>: data/hora ISO 8601 dentro de texto (<c>AAAA-MM-DDTHH:MM</c>, com segundos, fração e fuso opcionais) e valor do tipo data.</description></item>
        /// <item><description><c>&lt;epoch-ms&gt;</c>: instante Unix em milissegundos (13 dígitos, de 2017 a 2042) logo depois do prefixo <c>REF</c>, o id de referência que o script do bloco gera com <c>"REF" + Date.now()</c> quando a configuração não traz um. O prefixo continua no texto (<c>REF&lt;epoch-ms&gt;</c>); número de 13 dígitos sem esse prefixo é comparado como veio.</description></item>
        /// <item><description><c>&lt;epoch-s&gt;</c>: valor inteiro da propriedade <c>timestamp</c>, o instante Unix em segundos (<c>Math.floor(Date.now() / 1000)</c>) que o script de feedback grava em <c>payment.timestamp</c> da mensagem de status.</description></item>
        /// </list>
        /// Nomes de propriedade passam pelas mesmas regras de texto; se dois nomes viram o mesmo marcador, recebem o sufixo <c>#2</c>, <c>#3</c>... na ordem do valor normalizado.
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
        /// Devolve uma cópia normalizada de <paramref name="value"/>, sem alterar o original.
        /// </summary>
        /// <param name="value">JSON capturado da execução.</param>
        /// <param name="runSecrets">Segredos e tokens gerados nesta execução (mínimo de 8 caracteres cada), trocados por <c>&lt;segredo&gt;</c> onde aparecerem.</param>
        public static JToken Normalize(JToken value, IEnumerable<string> runSecrets = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return NormalizeToken(value, PrepareSecrets(runSecrets));
        }

        /// <summary>
        /// Lê JSON sem converter datas nem perder casas decimais, rejeitando propriedade duplicada e conteúdo além do fim.
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
        /// Texto canônico do JSON normalizado: indentado com 2 espaços, quebra de linha <c>\n</c> e <c>\n</c> no fim.
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
        /// Regra de volatilidade: troca por <see cref="Marker"/> o valor inteiro das propriedades em <see cref="PropertyNames"/>
        /// ou cada trecho de texto que casa com <see cref="TextPattern"/>.
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

            /// <summary>Marcador fixo que entra no lugar do valor volátil.</summary>
            public string Marker { get; }

            /// <summary>Nomes de propriedade cujo valor inteiro é volátil; vazio quando a regra é por padrão de texto.</summary>
            public IReadOnlyCollection<string> PropertyNames => _propertyNames;

            /// <summary>Quando verdadeiro, a regra só vale para propriedades de um objeto de envelope.</summary>
            public bool EnvelopeOnly { get; }

            /// <summary>Padrão procurado dentro de textos; nulo quando a regra é por nome de propriedade.</summary>
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
