using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Xunit.Sdk;

namespace Take.Blip.Builder.UnitTests.PaymentBlock.Comparison
{
    /// <summary>
    /// Compares the output of an engine run, normalized by <see cref="JsonNormalizer"/>, with the approved reference
    /// <c>PaymentBlock/References/&lt;name&gt;.json</c>. A difference in a non-volatile field or a missing reference fails the test.
    /// With <c>RECORD_REFERENCES=1</c>, <c>Take.Blip.Builder.UnitTests</c> writes the normalized version to the source folder instead
    /// of comparing; compiled with <c>ENGINE_PACKAGE</c>, it never writes.
    /// </summary>
    public sealed class ReferenceComparer
    {
        /// <summary>Environment variable that, set to <c>1</c>, writes the references instead of comparing.</summary>
        public const string RECORD_VARIABLE = "RECORD_REFERENCES";

        /// <summary>Folder of the Payments block files, in the build output and in the test project.</summary>
        public const string PAYMENT_BLOCK_FOLDER = "PaymentBlock";

        /// <summary>References folder, inside <see cref="PAYMENT_BLOCK_FOLDER"/>.</summary>
        public const string REFERENCES_FOLDER = "References";

        private const string JSON_EXTENSION = ".json";
        private const int MAX_REPORTED_DIFFERENCES = 20;
        private static readonly TimeSpan REGEX_TIMEOUT = TimeSpan.FromSeconds(1);
        private static readonly Regex NAME_PATTERN = new Regex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant, REGEX_TIMEOUT);
        private static readonly Regex IDENTIFIER_PATTERN = new Regex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant, REGEX_TIMEOUT);

        private readonly string _referenceDirectory;
#if !ENGINE_PACKAGE
        private const string PROJECT_FILE = "Take.Blip.Builder.UnitTests.csproj";
        private static readonly Encoding UTF8_WITHOUT_BOM = new UTF8Encoding(false, true);
        private readonly string _recordDirectory;
#endif

        /// <summary>
        /// Creates the comparer.
        /// </summary>
        /// <param name="referenceDirectory">Folder the references are read from.</param>
        /// <param name="recordDirectory">Folder the references are written to when <paramref name="record"/> is true.</param>
        /// <param name="record">Writes the normalized version instead of comparing. Ignored when compiled with <c>ENGINE_PACKAGE</c>.</param>
        public ReferenceComparer(string referenceDirectory, string recordDirectory = null, bool record = false)
        {
            if (string.IsNullOrWhiteSpace(referenceDirectory))
            {
                throw new ArgumentException("The reference folder is required.", nameof(referenceDirectory));
            }

            if (record && string.IsNullOrWhiteSpace(recordDirectory))
            {
                throw new ArgumentException("The record folder is required when recording is enabled.", nameof(recordDirectory));
            }

            _referenceDirectory = referenceDirectory;
#if ENGINE_PACKAGE
            IsRecording = false;
#else
            _recordDirectory = recordDirectory;
            IsRecording = record;
#endif
        }

        /// <summary>True when the comparer writes the references instead of comparing.</summary>
        public bool IsRecording { get; }

        /// <summary>
        /// Default comparer: reads <c>PaymentBlock/References/</c> from the build output and, with <c>RECORD_REFERENCES=1</c> in
        /// <c>Take.Blip.Builder.UnitTests</c>, writes to <c>src/Take.Blip.Builder.UnitTests/PaymentBlock/References/</c>.
        /// </summary>
        public static ReferenceComparer FromBuildOutput()
        {
            var referenceDirectory = Path.Combine(AppContext.BaseDirectory, PAYMENT_BLOCK_FOLDER, REFERENCES_FOLDER);
#if ENGINE_PACKAGE
            return new ReferenceComparer(referenceDirectory);
#else
            var record = string.Equals(Environment.GetEnvironmentVariable(RECORD_VARIABLE), "1", StringComparison.Ordinal);
            return new ReferenceComparer(referenceDirectory, record ? FindSourceReferenceDirectory() : null, record);
#endif
        }

        /// <summary>
        /// Normalizes <paramref name="actual"/> and compares it with the reference <paramref name="name"/>. Throws <see cref="XunitException"/>
        /// with the diverging JSON paths (without values) or, if the reference does not exist, with the recording instruction.
        /// </summary>
        /// <param name="name">Reference name, in lowercase and hyphens (e.g. <c>v1-checkout-pix</c>).</param>
        /// <param name="actual">JSON captured from the run.</param>
        /// <param name="runSecrets">Secrets and tokens generated in this run.</param>
        public void AssertMatches(string name, JToken actual, IEnumerable<string> runSecrets = null)
        {
            if (name == null || !NAME_PATTERN.IsMatch(name))
            {
                throw new ArgumentException("Invalid reference name: use lowercase letters, digits and hyphens.", nameof(name));
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
                    "Reference '" + name + "' missing at " + referencePath + ". Record it by running Take.Blip.Builder.UnitTests with "
                    + RECORD_VARIABLE + "=1, review the content and commit it under src/Take.Blip.Builder.UnitTests/"
                    + PAYMENT_BLOCK_FOLDER + "/" + REFERENCES_FOLDER + "/.");
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
                    return Path.Combine(directory.FullName, PAYMENT_BLOCK_FOLDER, REFERENCES_FOLDER);
                }
            }

            throw new InvalidOperationException(
                RECORD_VARIABLE + "=1 requires running Take.Blip.Builder.UnitTests inside the repository: " + PROJECT_FILE + " was not found above "
                + AppContext.BaseDirectory + ".");
        }
#endif

        private static void CollectDifferences(JToken expected, JToken actual, string path, DifferenceLog differences)
        {
            if (expected.Type != actual.Type)
            {
                differences.Add(path, "different type (reference " + TypeName(expected) + ", output " + TypeName(actual) + ")");
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
                        differences.Add(path, "different value");
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
                    differences.Add(propertyPath, "field missing from output");
                }
            }

            foreach (var property in actual.Properties())
            {
                if (!expected.ContainsKey(property.Name))
                {
                    differences.Add(ChildPath(path, property.Name), "extra field in output");
                }
            }
        }

        private static void CollectArrayDifferences(JArray expected, JArray actual, string path, DifferenceLog differences)
        {
            if (expected.Count != actual.Count)
            {
                differences.Add(path, "different size (reference " + expected.Count.ToString(CultureInfo.InvariantCulture)
                    + ", output " + actual.Count.ToString(CultureInfo.InvariantCulture) + ")");
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
                builder.Append("The engine output differs from reference '").Append(name).Append("' at ")
                    .Append(Total.ToString(CultureInfo.InvariantCulture))
                    .Append(" point(s); values are not printed to avoid exposing sensitive data:\n");
                foreach (var line in _lines)
                {
                    builder.Append("  - ").Append(line).Append('\n');
                }

                if (Total > _lines.Count)
                {
                    builder.Append("  - ... and ").Append((Total - _lines.Count).ToString(CultureInfo.InvariantCulture)).Append(" more\n");
                }

                builder.Append("If the change is expected, re-record it in Take.Blip.Builder.UnitTests with ").Append(RECORD_VARIABLE)
                    .Append("=1 and review the diff before committing.");
                return builder.ToString();
            }
        }
    }
}
