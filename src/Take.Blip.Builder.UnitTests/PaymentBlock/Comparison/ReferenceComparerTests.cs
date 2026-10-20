using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;
using Xunit.Sdk;

namespace Take.Blip.Builder.UnitTests.PaymentBlock.Comparison
{
    public sealed class ReferenceComparerTests : IDisposable
    {
        private const string CATEGORY = "Category";
        private const string COMPARER = "comparer";
        private const string VOLATILE_PASSES = "comparer-volatile-passes";
        private const string DIFFERENCE_FAILS = "comparer-difference-fails";
        private const string MISSING_FAILS = "comparer-missing-fails";
        private const string RECORDING_WRITES = "comparer-recording-writes";
        private const string REFERENCE_NAME = "v1-comparer";
        private const string CHECKOUT_URI = "/payments/checkout";

        private static readonly TimeSpan REGEX_TIMEOUT = TimeSpan.FromSeconds(1);
        private static readonly Regex GUID_PATTERN = new Regex("[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, REGEX_TIMEOUT);
        private static readonly Regex DATE_TIME_PATTERN = new Regex("[0-9]{4}-[0-9]{2}-[0-9]{2}[T ][0-9]{2}:[0-9]{2}", RegexOptions.CultureInvariant, REGEX_TIMEOUT);
        private static readonly string ZERO_GUID = Guid.Empty.ToString();
        private static readonly byte[] UTF8_BOM = { 0xEF, 0xBB, 0xBF };

        private readonly DirectoryInfo _referenceDirectory;
        private readonly DirectoryInfo _recordDirectory;

        public ReferenceComparerTests()
        {
            _referenceDirectory = Directory.CreateTempSubdirectory("paymentblock-references-");
            _recordDirectory = Directory.CreateTempSubdirectory("paymentblock-recording-");
        }

        public void Dispose()
        {
            _referenceDirectory.Delete(true);
            _recordDirectory.Delete(true);
        }

        [Fact]
        [Trait(CATEGORY, COMPARER)]
        [Trait(CATEGORY, VOLATILE_PASSES)]
        public void VolatileFieldsDifferentFromReferenceShouldPass()
        {
            // Arrange
            var recorded = SyntheticRun.Create(TimeSpan.Zero, CHECKOUT_URI);
            var current = SyntheticRun.Create(TimeSpan.FromDays(3).Add(TimeSpan.FromMinutes(17)), CHECKOUT_URI);
            WriteReference(recorded);
            var comparer = new ReferenceComparer(_referenceDirectory.FullName);

            // Act
            var act = new Action(() => comparer.AssertMatches(REFERENCE_NAME, current.Json, current.Secrets));

            // Assert
            Should.NotThrow(act);
            comparer.IsRecording.ShouldBeFalse();
        }

        [Fact]
        [Trait(CATEGORY, COMPARER)]
        [Trait(CATEGORY, VOLATILE_PASSES)]
        public void NormalizedTextShouldBeDeterministicAndFreeOfVolatileValues()
        {
            // Arrange
            var first = SyntheticRun.Create(TimeSpan.Zero, CHECKOUT_URI);
            var second = SyntheticRun.Create(TimeSpan.FromHours(30), CHECKOUT_URI);

            // Act
            var firstText = Canonical(first);
            var secondText = Canonical(second);

            // Assert
            secondText.ShouldBe(firstText);
            foreach (Match guid in GUID_PATTERN.Matches(firstText))
            {
                guid.Value.ShouldBe(ZERO_GUID);
            }

            DATE_TIME_PATTERN.IsMatch(firstText).ShouldBeFalse();
            firstText.ShouldContain(ZERO_GUID);
            firstText.ShouldContain("REF<epoch-ms>");
            firstText.ShouldContain("\"timestamp\": \"<epoch-s>\"");
            firstText.ShouldContain("\"amount\": 1999");
            firstText.ShouldNotContain(first.Token);
            firstText.ShouldNotContain(first.Secret);
            firstText.ShouldNotContain("\r");
            firstText.ShouldEndWith("}\n");
            JsonNormalizer.ToCanonicalText(JsonNormalizer.Normalize(JsonNormalizer.Parse(firstText))).ShouldBe(firstText);
        }

        [Fact]
        [Trait(CATEGORY, COMPARER)]
        [Trait(CATEGORY, DIFFERENCE_FAILS)]
        public void NonVolatileFieldDifferentFromReferenceShouldFailWithJsonPath()
        {
            // Arrange
            var recorded = SyntheticRun.Create(TimeSpan.Zero, CHECKOUT_URI);
            var current = SyntheticRun.Create(TimeSpan.FromMinutes(5), "/payments/status");
            WriteReference(recorded);
            var comparer = new ReferenceComparer(_referenceDirectory.FullName);

            // Act
            var exception = Should.Throw<XunitException>(() => comparer.AssertMatches(REFERENCE_NAME, current.Json, current.Secrets));

            // Assert
            exception.Message.ShouldContain("$.commands[0].uri: different value");
            exception.Message.ShouldContain(" 1 point(s)");
            exception.Message.ShouldNotContain("/payments/status");
            AssertNoSecret(exception.Message, recorded, current);
        }

        [Fact]
        [Trait(CATEGORY, COMPARER)]
        [Trait(CATEGORY, DIFFERENCE_FAILS)]
        public void MissingExtraAndRetypedFieldsShouldFailWithEveryJsonPath()
        {
            // Arrange
            var recorded = SyntheticRun.Create(TimeSpan.Zero, CHECKOUT_URI);
            var current = SyntheticRun.Create(TimeSpan.FromMinutes(9), CHECKOUT_URI);
            WriteReference(recorded);
            var resource = (JObject)current.Json["commands"][0]["resource"];
            resource.Remove("amount");
            resource["installments"] = "1x " + current.Secret;
            resource["paymentKey"] = 0;
            ((JObject)current.Json["commands"][0]["metadata"])["#tunnel.owner"] = "store@msging.net";
            ((JArray)current.Json["http"]).Add(new JObject { ["method"] = "GET" });
            var comparer = new ReferenceComparer(_referenceDirectory.FullName);

            // Act
            var exception = Should.Throw<XunitException>(() => comparer.AssertMatches(REFERENCE_NAME, current.Json, current.Secrets));

            // Assert
            exception.Message.ShouldContain("$.commands[0].resource.amount: field missing from output");
            exception.Message.ShouldContain("$.commands[0].resource.installments: extra field in output");
            exception.Message.ShouldContain("$.commands[0].resource.paymentKey: different type (reference string, output integer)");
            exception.Message.ShouldContain("$.commands[0].metadata['#tunnel.owner']: extra field in output");
            exception.Message.ShouldContain("$.http: different size (reference 1, output 2)");
            AssertNoSecret(exception.Message, recorded, current);
        }

        [Fact]
        [Trait(CATEGORY, COMPARER)]
        [Trait(CATEGORY, DIFFERENCE_FAILS)]
        public void ThirteenDigitNumberOutsideGeneratedReferenceIdShouldNotBeMasked()
        {
            // Arrange
            var recorded = SyntheticRun.Create(TimeSpan.Zero, CHECKOUT_URI);
            var current = SyntheticRun.Create(TimeSpan.FromMinutes(3), CHECKOUT_URI);
            SetOrderNumber(recorded, 1712345678901L);
            SetOrderNumber(current, 1798765432109L);
            WriteReference(recorded);
            var comparer = new ReferenceComparer(_referenceDirectory.FullName);

            // Act
            var exception = Should.Throw<XunitException>(() => comparer.AssertMatches(REFERENCE_NAME, current.Json, current.Secrets));

            // Assert
            exception.Message.ShouldContain("$.commands[0].resource.orderNumber: different value");
            exception.Message.ShouldContain("$.commands[0].resource.orderLabel: different value");
            exception.Message.ShouldContain(" 2 point(s)");
            Canonical(recorded).ShouldContain("\"orderNumber\": 1712345678901");
            Canonical(recorded).ShouldContain("\"orderLabel\": \"order-1712345678901\"");
            AssertNoSecret(exception.Message, recorded, current);
        }

        [Fact]
        [Trait(CATEGORY, COMPARER)]
        [Trait(CATEGORY, MISSING_FAILS)]
        public void MissingReferenceShouldFailPointingToRecordVariable()
        {
            // Arrange
            var current = SyntheticRun.Create(TimeSpan.Zero, CHECKOUT_URI);
            var comparer = new ReferenceComparer(_referenceDirectory.FullName);

            // Act
            var exception = Should.Throw<XunitException>(() => comparer.AssertMatches(REFERENCE_NAME, current.Json, current.Secrets));

            // Assert
            exception.Message.ShouldContain("'" + REFERENCE_NAME + "' missing");
            exception.Message.ShouldContain(ReferenceComparer.RECORD_VARIABLE + "=1");
            _referenceDirectory.GetFileSystemInfos().ShouldBeEmpty();
            AssertNoSecret(exception.Message, current, current);
        }

        [Fact]
        [Trait(CATEGORY, COMPARER)]
        [Trait(CATEGORY, MISSING_FAILS)]
        public void MissingReferenceFolderShouldFailPointingToRecordVariable()
        {
            // Arrange
            var current = SyntheticRun.Create(TimeSpan.Zero, CHECKOUT_URI);
            var missingFolder = Path.Combine(_referenceDirectory.FullName, ReferenceComparer.REFERENCES_FOLDER);
            var comparer = new ReferenceComparer(missingFolder);

            // Act
            var exception = Should.Throw<XunitException>(() => comparer.AssertMatches(REFERENCE_NAME, current.Json, current.Secrets));

            // Assert
            exception.Message.ShouldContain(ReferenceComparer.RECORD_VARIABLE + "=1");
            Directory.Exists(missingFolder).ShouldBeFalse();
        }

        [Fact]
        [Trait(CATEGORY, COMPARER)]
        [Trait(CATEGORY, RECORDING_WRITES)]
        public void RecordingShouldWriteCanonicalReferenceWithoutComparing()
        {
            // Arrange
            var run = SyntheticRun.Create(TimeSpan.Zero, CHECKOUT_URI);
            var recordFolder = Path.Combine(_recordDirectory.FullName, ReferenceComparer.REFERENCES_FOLDER);
            var recordedPath = Path.Combine(recordFolder, REFERENCE_NAME + ".json");
            var comparer = new ReferenceComparer(_referenceDirectory.FullName, recordFolder, record: true);

            // Act
            var act = new Action(() => comparer.AssertMatches(REFERENCE_NAME, run.Json, run.Secrets));

            // Assert
            Should.NotThrow(act);
            comparer.IsRecording.ShouldBeTrue();
            File.Exists(recordedPath).ShouldBeTrue();
            var bytes = File.ReadAllBytes(recordedPath);
            bytes.Length.ShouldBeGreaterThan(UTF8_BOM.Length);
            bytes.AsSpan(0, UTF8_BOM.Length).SequenceEqual(UTF8_BOM).ShouldBeFalse("the recorded reference starts with a UTF-8 BOM");
            var text = Encoding.UTF8.GetString(bytes);
            text.ShouldBe(Canonical(run));
            text.ShouldNotContain(run.Token);
            text.ShouldNotContain(run.Secret);
            _referenceDirectory.GetFileSystemInfos().ShouldBeEmpty();
        }

        private void WriteReference(SyntheticRun run)
        {
            File.WriteAllText(Path.Combine(_referenceDirectory.FullName, REFERENCE_NAME + ".json"), Canonical(run), new UTF8Encoding(false));
        }

        private static void SetOrderNumber(SyntheticRun run, long orderNumber)
        {
            var resource = (JObject)run.Json["commands"][0]["resource"];
            resource["orderNumber"] = orderNumber;
            resource["orderLabel"] = "order-" + orderNumber.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string Canonical(SyntheticRun run)
        {
            return JsonNormalizer.ToCanonicalText(JsonNormalizer.Normalize(run.Json, run.Secrets));
        }

        private static void AssertNoSecret(string message, SyntheticRun recorded, SyntheticRun current)
        {
            message.ShouldNotContain(recorded.Token);
            message.ShouldNotContain(recorded.Secret);
            message.ShouldNotContain(current.Token);
            message.ShouldNotContain(current.Secret);
        }

        private sealed class SyntheticRun
        {
            private SyntheticRun(JObject json, string token, string secret)
            {
                Json = json;
                Token = token;
                Secret = secret;
            }

            public JObject Json { get; }

            public string Token { get; }

            public string Secret { get; }

            public string[] Secrets => new[] { Token, Secret };

            public static SyntheticRun Create(TimeSpan shift, string checkoutUri)
            {
                var now = DateTimeOffset.UtcNow.Add(shift);
                var envelopeId = Guid.NewGuid().ToString();
                var token = "tok-" + Guid.NewGuid().ToString("N");
                var secret = "sec-" + Guid.NewGuid().ToString("N");
                var json = new JObject
                {
                    ["commands"] = new JArray
                    {
                        new JObject
                        {
                            ["id"] = envelopeId,
                            ["to"] = "postmaster@msging.net",
                            ["method"] = "set",
                            ["uri"] = checkoutUri,
                            ["metadata"] = new JObject
                            {
                                ["#envelope.sendToServerRequired"] = "true",
                                ["#uniqueId"] = "u-" + envelopeId,
                            },
                            ["resource"] = new JObject
                            {
                                ["referenceId"] = "REF" + now.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
                                ["orderId"] = Guid.NewGuid().ToString(),
                                ["createdAt"] = now.ToString("o", System.Globalization.CultureInfo.InvariantCulture),
                                ["scheduledAt"] = new JValue(now.AddMinutes(10)),
                                ["paymentKey"] = ZERO_GUID,
                                ["amount"] = 1999,
                                ["timestamp"] = now.ToUnixTimeSeconds(),
                                ["botToken"] = token,
                            },
                        },
                    },
                    ["http"] = new JArray
                    {
                        new JObject
                        {
                            ["method"] = "POST",
                            ["uri"] = "https://checkout.hmg.example.test/api/v1/token",
                            ["headers"] = new JObject { ["Authorization"] = "Bearer " + token },
                            ["body"] = "{\"botShortName\":\"store\",\"secret\":\"" + secret + "\"}",
                        },
                    },
                };
                return new SyntheticRun(json, token, secret);
            }
        }
    }
}
