using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Xunit.Sdk;

namespace Take.Blip.Builder.PaymentBlock.Tests.Comparison
{
    /// <summary>
    /// Compara a saída de uma execução do motor, normalizada por <see cref="JsonNormalizer"/>, com a referência aprovada
    /// <c>Referencias/&lt;nome&gt;.json</c>. Diferença em campo não volátil ou referência ausente reprovam o teste.
    /// Com <c>REFERENCIAS_GRAVAR=1</c>, o projeto principal grava a versão normalizada na pasta-fonte em vez de comparar;
    /// compilado com <c>ENGINE_PACKAGE</c>, nunca grava.
    /// </summary>
    public sealed class ReferenceComparer
    {
        /// <summary>Variável de ambiente que, com o valor <c>1</c>, grava as referências em vez de comparar.</summary>
        public const string RECORD_VARIABLE = "REFERENCIAS_GRAVAR";

        /// <summary>Pasta das referências, na saída do build e no projeto principal.</summary>
        public const string REFERENCES_FOLDER = "Referencias";

        private const string JSON_EXTENSION = ".json";
        private const int MAX_REPORTED_DIFFERENCES = 20;
        private static readonly TimeSpan REGEX_TIMEOUT = TimeSpan.FromSeconds(1);
        private static readonly Regex NAME_PATTERN = new Regex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant, REGEX_TIMEOUT);
        private static readonly Regex IDENTIFIER_PATTERN = new Regex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant, REGEX_TIMEOUT);

        private readonly string _referenceDirectory;
#if !ENGINE_PACKAGE
        private const string PROJECT_FILE = "Take.Blip.Builder.PaymentBlock.Tests.csproj";
        private static readonly Encoding UTF8_WITHOUT_BOM = new UTF8Encoding(false, true);
        private readonly string _recordDirectory;
#endif

        /// <summary>
        /// Cria o comparador.
        /// </summary>
        /// <param name="referenceDirectory">Pasta de onde as referências são lidas.</param>
        /// <param name="recordDirectory">Pasta onde as referências são gravadas quando <paramref name="record"/> é verdadeiro.</param>
        /// <param name="record">Grava a versão normalizada em vez de comparar. Ignorado quando compilado com <c>ENGINE_PACKAGE</c>.</param>
        public ReferenceComparer(string referenceDirectory, string recordDirectory = null, bool record = false)
        {
            if (string.IsNullOrWhiteSpace(referenceDirectory))
            {
                throw new ArgumentException("A pasta de referências é obrigatória.", nameof(referenceDirectory));
            }

            if (record && string.IsNullOrWhiteSpace(recordDirectory))
            {
                throw new ArgumentException("A pasta de gravação é obrigatória quando a gravação está ligada.", nameof(recordDirectory));
            }

            _referenceDirectory = referenceDirectory;
#if ENGINE_PACKAGE
            IsRecording = false;
#else
            _recordDirectory = recordDirectory;
            IsRecording = record;
#endif
        }

        /// <summary>Verdadeiro quando o comparador grava as referências em vez de comparar.</summary>
        public bool IsRecording { get; }

        /// <summary>
        /// Comparador padrão: lê <c>Referencias/</c> da saída do build e, com <c>REFERENCIAS_GRAVAR=1</c> no projeto principal,
        /// grava em <c>src/Take.Blip.Builder.PaymentBlock.Tests/Referencias/</c>.
        /// </summary>
        public static ReferenceComparer FromBuildOutput()
        {
            var referenceDirectory = Path.Combine(AppContext.BaseDirectory, REFERENCES_FOLDER);
#if ENGINE_PACKAGE
            return new ReferenceComparer(referenceDirectory);
#else
            var record = string.Equals(Environment.GetEnvironmentVariable(RECORD_VARIABLE), "1", StringComparison.Ordinal);
            return new ReferenceComparer(referenceDirectory, record ? FindSourceReferenceDirectory() : null, record);
#endif
        }

        /// <summary>
        /// Normaliza <paramref name="actual"/> e compara com a referência <paramref name="name"/>. Lança <see cref="XunitException"/>
        /// com os caminhos JSON divergentes (sem valores) ou, se a referência não existir, com a instrução de gravação.
        /// </summary>
        /// <param name="name">Nome da referência, em minúsculas e hífens (ex.: <c>v1-checkout-pix</c>).</param>
        /// <param name="actual">JSON capturado da execução.</param>
        /// <param name="runSecrets">Segredos e tokens gerados nesta execução.</param>
        public void AssertMatches(string name, JToken actual, IEnumerable<string> runSecrets = null)
        {
            if (name == null || !NAME_PATTERN.IsMatch(name))
            {
                throw new ArgumentException("Nome de referência inválido: use minúsculas, dígitos e hífens.", nameof(name));
            }

            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            var canonicalText = JsonNormalizer.ToCanonicalText(JsonNormalizer.Normalize(actual, runSecrets));
#if !ENGINE_PACKAGE
            if (IsRecording)
            {
                Directory.CreateDirectory(_recordDirectory);
                File.WriteAllText(Path.Combine(_recordDirectory, name + JSON_EXTENSION), canonicalText, UTF8_WITHOUT_BOM);
                return;
            }
#endif
            var referencePath = Path.Combine(_referenceDirectory, name + JSON_EXTENSION);
            if (!File.Exists(referencePath))
            {
                throw new XunitException(
                    "Referência '" + name + "' ausente em " + referencePath + ". Grave-a rodando o projeto principal com "
                    + RECORD_VARIABLE + "=1, revise o conteúdo e versione em src/Take.Blip.Builder.PaymentBlock.Tests/"
                    + REFERENCES_FOLDER + "/.");
            }

            var expected = JsonNormalizer.Normalize(JsonNormalizer.Parse(File.ReadAllText(referencePath, Encoding.UTF8)));
            var differences = new DifferenceLog();
            CollectDifferences(expected, JsonNormalizer.Parse(canonicalText), "$", differences);
            if (differences.Total > 0)
            {
                throw new XunitException(differences.ToMessage(name));
            }
        }

#if !ENGINE_PACKAGE
        private static string FindSourceReferenceDirectory()
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, PROJECT_FILE)))
                {
                    return Path.Combine(directory.FullName, REFERENCES_FOLDER);
                }
            }

            throw new InvalidOperationException(
                RECORD_VARIABLE + "=1 exige rodar o projeto principal dentro do repositório: " + PROJECT_FILE + " não foi encontrado acima de "
                + AppContext.BaseDirectory + ".");
        }
#endif

        private static void CollectDifferences(JToken expected, JToken actual, string path, DifferenceLog differences)
        {
            if (expected.Type != actual.Type)
            {
                differences.Add(path, "tipo diferente (referência " + TypeName(expected) + ", saída " + TypeName(actual) + ")");
                return;
            }

            switch (expected)
            {
                case JObject expectedObject:
                    CollectObjectDifferences(expectedObject, (JObject)actual, path, differences);
                    return;
                case JArray expectedArray:
                    CollectArrayDifferences(expectedArray, (JArray)actual, path, differences);
                    return;
                default:
                    if (!JToken.DeepEquals(expected, actual))
                    {
                        differences.Add(path, "valor diferente");
                    }

                    return;
            }
        }

        private static void CollectObjectDifferences(JObject expected, JObject actual, string path, DifferenceLog differences)
        {
            foreach (var property in expected.Properties())
            {
                var propertyPath = ChildPath(path, property.Name);
                if (actual.TryGetValue(property.Name, out var actualValue))
                {
                    CollectDifferences(property.Value, actualValue, propertyPath, differences);
                }
                else
                {
                    differences.Add(propertyPath, "campo ausente na saída");
                }
            }

            foreach (var property in actual.Properties())
            {
                if (!expected.ContainsKey(property.Name))
                {
                    differences.Add(ChildPath(path, property.Name), "campo a mais na saída");
                }
            }
        }

        private static void CollectArrayDifferences(JArray expected, JArray actual, string path, DifferenceLog differences)
        {
            if (expected.Count != actual.Count)
            {
                differences.Add(path, "tamanho diferente (referência " + expected.Count.ToString(CultureInfo.InvariantCulture)
                    + ", saída " + actual.Count.ToString(CultureInfo.InvariantCulture) + ")");
            }

            var common = Math.Min(expected.Count, actual.Count);
            for (var index = 0; index < common; index++)
            {
                CollectDifferences(expected[index], actual[index], path + "[" + index.ToString(CultureInfo.InvariantCulture) + "]", differences);
            }
        }

        private static string ChildPath(string parent, string name)
        {
            return IDENTIFIER_PATTERN.IsMatch(name)
                ? parent + "." + name
                : parent + "['" + name.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal) + "']";
        }

        private static string TypeName(JToken token)
        {
            return token.Type.ToString().ToLowerInvariant();
        }

        private sealed class DifferenceLog
        {
            private readonly List<string> _lines = new List<string>(MAX_REPORTED_DIFFERENCES);

            public int Total { get; private set; }

            public void Add(string path, string description)
            {
                Total++;
                if (_lines.Count < MAX_REPORTED_DIFFERENCES)
                {
                    _lines.Add(path + ": " + description);
                }
            }

            public string ToMessage(string name)
            {
                var builder = new StringBuilder();
                builder.Append("A saída do motor diverge da referência '").Append(name).Append("' em ")
                    .Append(Total.ToString(CultureInfo.InvariantCulture))
                    .Append(" ponto(s); os valores não são impressos para não expor dado sensível:\n");
                foreach (var line in _lines)
                {
                    builder.Append("  - ").Append(line).Append('\n');
                }

                if (Total > _lines.Count)
                {
                    builder.Append("  - ... e mais ").Append((Total - _lines.Count).ToString(CultureInfo.InvariantCulture)).Append('\n');
                }

                builder.Append("Se a mudança for esperada, regrave no projeto principal com ").Append(RECORD_VARIABLE)
                    .Append("=1 e revise o diff antes de versionar.");
                return builder.ToString();
            }
        }
    }
}
