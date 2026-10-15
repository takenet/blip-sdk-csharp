using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.PaymentBlock.Tests.Support
{
    /// <summary>
    /// Monta, em memória, o fluxo executado pelo motor a partir de <c>Fixtures/</c>, conforme a montagem do estado descrita
    /// no <c>ORIGEM.md</c>: o <c>model</c> de <c>&lt;fluxo&gt;-estado.json</c> com as referências <c>$fixture</c>/<c>$path</c>
    /// resolvidas, os SetVariable de configuração preenchidos com uma linha de <c>configuracoes-v1.json</c>, um estado
    /// inicial que leva ao bloco e um estado de saída para cada saída do bloco sem destino. Os arquivos não mudam.
    /// </summary>
    public static class FlowAssembler
    {
        /// <summary>Pasta das fixtures, na saída do build.</summary>
        public const string FIXTURES_FOLDER = "Fixtures";

        /// <summary>Matriz de configurações do bloco.</summary>
        public const string CONFIGURATIONS_FILE = "configuracoes-v1.json";

        /// <summary>Estado raiz do fluxo montado: espera a mensagem do cliente e segue para o bloco.</summary>
        public const string ENTRY_STATE_ID = "inicio";

        private const string STATE_FILE_SUFFIX = "-estado.json";
        private const string FIXTURE_REFERENCE = "$fixture";
        private const string FIXTURE_PATH = "$path";
        private const string EXIT_STATE_PREFIX = "saida-";
        private const string DEFAULT_EXIT_NAME = "padrao";
        private const string STATE_ID = "stateId";
        private const string SETTINGS = "settings";

        private static readonly string[] CONFIGURATION_VARIABLES = { "configurationData", "paymentMethodData", "customizeData" };

        /// <summary>Id do estado de saída ligado à saída do bloco cuja primeira condição vale <paramref name="outputValue"/>.</summary>
        public static string ExitStateId(string outputValue)
        {
            if (string.IsNullOrWhiteSpace(outputValue))
            {
                throw new ArgumentException("O valor da saída é obrigatório.", nameof(outputValue));
            }

            return EXIT_STATE_PREFIX + outputValue.ToLowerInvariant();
        }

        /// <summary>
        /// Linha de <c>configuracoes-v1.json</c> com a <paramref name="configuration"/> (coluna <c>configuracao</c>) e o
        /// <paramref name="feedback"/> informados.
        /// </summary>
        public static JObject LoadConfiguration(string configuration, bool feedback)
        {
            if (!(ReadFixture(CONFIGURATIONS_FILE) is JArray rows))
            {
                throw new InvalidDataException(CONFIGURATIONS_FILE + " deveria ser uma lista.");
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
                "Configuração ausente em " + CONFIGURATIONS_FILE + ": " + configuration + " (feedback " + feedback + ").",
                nameof(configuration));
        }

        /// <summary>Monta o fluxo de <paramref name="referenceFlow"/> com a linha de configuração informada.</summary>
        public static Flow Assemble(string referenceFlow, JObject configuration)
        {
            if (string.IsNullOrWhiteSpace(referenceFlow))
            {
                throw new ArgumentException("O fluxo de referência é obrigatório.", nameof(referenceFlow));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var model = ReadFixture(referenceFlow + STATE_FILE_SUFFIX)["model"]
                        ?? throw new InvalidDataException(referenceFlow + STATE_FILE_SUFFIX + " sem a propriedade model.");
            var state = ResolveFixtureReferences(model.DeepClone()) as JObject
                        ?? throw new InvalidDataException("O model do estado deveria ser um objeto.");

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
                throw new InvalidDataException("O estado do bloco não tem inputActions.");
            }

            foreach (var variable in CONFIGURATION_VARIABLES)
            {
                var value = configuration[variable]
                            ?? throw new InvalidDataException("A linha de configuração não tem " + variable + ".");
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

            throw new InvalidDataException("O bloco não tem o SetVariable de " + variable + ".");
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
            var path = Path.Combine(AppContext.BaseDirectory, FIXTURES_FOLDER, fileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Fixture ausente na saída do build: " + fileName, path);
            }

            using (var reader = new JsonTextReader(File.OpenText(path)) { DateParseHandling = DateParseHandling.None })
            {
                return JToken.ReadFrom(reader);
            }
        }
    }
}
