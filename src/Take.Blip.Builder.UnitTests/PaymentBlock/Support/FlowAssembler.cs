using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.UnitTests.PaymentBlock.Support
{
    /// <summary>
    /// Assembles in memory the flow the engine runs from <c>PaymentBlock/Fixtures/</c>, following the state assembly described
    /// in <c>ORIGEM.md</c>: the <c>model</c> of <c>&lt;flow&gt;-estado.json</c> with the <c>$fixture</c>/<c>$path</c> references
    /// resolved, the configuration SetVariable actions filled with a row of <c>configuracoes-v1.json</c>, an initial state
    /// that leads to the block and an exit state for each block output without a destination. The files do not change.
    /// </summary>
    public static class FlowAssembler
    {
        /// <summary>Folder of the Payments block files, in the build output and in the test project.</summary>
        public const string PAYMENT_BLOCK_FOLDER = "PaymentBlock";

        /// <summary>Fixtures folder, inside <see cref="PAYMENT_BLOCK_FOLDER"/>.</summary>
        public const string FIXTURES_FOLDER = "Fixtures";

        /// <summary>Block configuration matrix.</summary>
        public const string CONFIGURATIONS_FILE = "configuracoes-v1.json";

        /// <summary>Root state of the assembled flow: waits for the customer message and moves on to the block.</summary>
        public const string ENTRY_STATE_ID = "inicio";

        private const string STATE_FILE_SUFFIX = "-estado.json";
        private const string FIXTURE_REFERENCE = "$fixture";
        private const string FIXTURE_PATH = "$path";
        private const string EXIT_STATE_PREFIX = "saida-";
        private const string DEFAULT_EXIT_NAME = "padrao";
        private const string STATE_ID = "stateId";
        private const string SETTINGS = "settings";

        private static readonly string[] CONFIGURATION_VARIABLES = { "configurationData", "paymentMethodData", "customizeData" };

        /// <summary>Id of the exit state linked to the block output whose first condition equals <paramref name="outputValue"/>.</summary>
        public static string ExitStateId(string outputValue)
        {
            if (string.IsNullOrWhiteSpace(outputValue))
            {
                throw new ArgumentException("The output value is required.", nameof(outputValue));
            }

            return EXIT_STATE_PREFIX + outputValue.ToLowerInvariant();
        }

        /// <summary>
        /// Row of <c>configuracoes-v1.json</c> with the given <paramref name="configuration"/> (<c>configuracao</c> column) and
        /// <paramref name="feedback"/>.
        /// </summary>
        public static JObject LoadConfiguration(string configuration, bool feedback)
        {
            if (!(ReadFixture(CONFIGURATIONS_FILE) is JArray rows))
            {
                throw new InvalidDataException(CONFIGURATIONS_FILE + " should be a list.");
            }

            foreach (var row in rows.Children<JObject>())
            {
                if (string.Equals((string)row["configuracao"], configuration, StringComparison.Ordinal)
                    && (bool?)row["feedback"] == feedback)
                {
                    return row;
                }
            }

            throw new ArgumentException(
                "Configuration missing from " + CONFIGURATIONS_FILE + ": " + configuration + " (feedback " + feedback + ").",
                nameof(configuration));
        }

        /// <summary>Assembles the <paramref name="referenceFlow"/> flow with the given configuration row.</summary>
        public static Flow Assemble(string referenceFlow, JObject configuration)
        {
            if (string.IsNullOrWhiteSpace(referenceFlow))
            {
                throw new ArgumentException("The reference flow is required.", nameof(referenceFlow));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var model = ReadFixture(referenceFlow + STATE_FILE_SUFFIX)["model"]
                        ?? throw new InvalidDataException(referenceFlow + STATE_FILE_SUFFIX + " has no model property.");
            var state = ResolveFixtureReferences(model.DeepClone()) as JObject
                        ?? throw new InvalidDataException("The state model should be an object.");

            ApplyConfiguration(state["inputActions"] as JArray, configuration);

            var states = new JArray { CreateEntryState((string)state["id"]), state };
            foreach (var exitState in ConnectExitStates(state))
            {
                states.Add(exitState);
            }

            var flow = new JObject
            {
                ["id"] = SyntheticData.FLOW_ID,
                ["states"] = states,
            };

            return Flow.ParseFromJson(flow.ToString(Formatting.None));
        }

        private static void ApplyConfiguration(JArray inputActions, JObject configuration)
        {
            if (inputActions == null)
            {
                throw new InvalidDataException("The block state has no inputActions.");
            }

            foreach (var variable in CONFIGURATION_VARIABLES)
            {
                var value = configuration[variable]
                            ?? throw new InvalidDataException("The configuration row has no " + variable + ".");
                FindSetVariableSettings(inputActions, variable)["value"] = value.ToString(Formatting.None);
            }
        }

        private static JObject FindSetVariableSettings(JArray actions, string variable)
        {
            foreach (var action in actions.Children<JObject>())
            {
                if (string.Equals((string)action["type"], "SetVariable", StringComparison.Ordinal)
                    && action[SETTINGS] is JObject settings
                    && string.Equals((string)settings["variable"], variable, StringComparison.Ordinal))
                {
                    return settings;
                }
            }

            throw new InvalidDataException("The block has no SetVariable for " + variable + ".");
        }

        private static JObject CreateEntryState(string blockStateId) => new JObject
        {
            ["id"] = ENTRY_STATE_ID,
            ["root"] = true,
            ["input"] = new JObject(),
            ["outputs"] = new JArray { new JObject { [STATE_ID] = blockStateId } },
        };

        private static List<JObject> ConnectExitStates(JObject state)
        {
            var exitStates = new List<JObject>();
            var created = new HashSet<string>(StringComparer.Ordinal);
            if (!(state["outputs"] is JArray outputs))
            {
                return exitStates;
            }

            foreach (var output in outputs.Children<JObject>())
            {
                if (!string.IsNullOrEmpty((string)output[STATE_ID]))
                {
                    continue;
                }

                var exitStateId = ExitStateId(FirstConditionValue(output) ?? DEFAULT_EXIT_NAME);
                output[STATE_ID] = exitStateId;
                if (created.Add(exitStateId))
                {
                    exitStates.Add(new JObject { ["id"] = exitStateId, ["input"] = new JObject() });
                }
            }

            return exitStates;
        }

        private static string FirstConditionValue(JObject output)
        {
            if (output["conditions"] is JArray conditions
                && conditions.Count > 0
                && conditions[0]["values"] is JArray values
                && values.Count > 0)
            {
                return (string)values[0];
            }

            return null;
        }

        private static JToken ResolveFixtureReferences(JToken token)
        {
            if (token is JObject reference && reference.ContainsKey(FIXTURE_REFERENCE))
            {
                var content = ReadFixture((string)reference[FIXTURE_REFERENCE]);
                var path = (string)reference[FIXTURE_PATH];
                var resolved = path == null ? content : content.SelectToken(path, true);
                return ResolveFixtureReferences(resolved.DeepClone());
            }

            if (token is JObject obj)
            {
                foreach (var property in new List<JProperty>(obj.Properties()))
                {
                    property.Value = ResolveFixtureReferences(property.Value);
                }
            }
            else if (token is JArray array)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    array[i] = ResolveFixtureReferences(array[i]);
                }
            }

            return token;
        }

        private static JToken ReadFixture(string fileName)
        {
            var path = Path.Combine(AppContext.BaseDirectory, PAYMENT_BLOCK_FOLDER, FIXTURES_FOLDER, fileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Fixture missing from the build output: " + fileName, path);
            }

            using (var reader = new JsonTextReader(File.OpenText(path)) { DateParseHandling = DateParseHandling.None })
            {
                return JToken.ReadFrom(reader);
            }
        }
    }
}
