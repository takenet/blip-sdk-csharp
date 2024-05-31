using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Actions.ExecuteScript;
using Take.Blip.Builder.Actions.ExecuteScriptV2;

namespace Take.Blip.Builder.Benchmark.Actions;

internal static class Settings
{
    private const string SIMPLE_SCRIPT = """
                                         function run() {
                                             return 'a';
                                         }
                                         """;

    private const string LOOP_SCRIPT = """
                                       function run() {
                                           let a = 0;
                                       
                                           while (a < 1000) {
                                               a++;
                                           }
                                       
                                           return a;
                                       }
                                       """;

    private const string JSON_SCRIPT = """
                                       function run() {
                                           const json = { 'a': 1, 'b': 'c', 'd' : ['1', '2'], 'e': { 'f': 1 } };
                                       
                                           let stringJson = JSON.stringify(json);
                                       
                                           let parsedJson = JSON.parse(stringJson);
                                       
                                           return parsedJson.a + parsedJson.e.f;
                                       }
                                       """;

    private const string MATH_SCRIPT = """
                                       function add(a, b) {
                                           return a + b;
                                       }

                                       function subtract(a, b) {
                                           return a - b;
                                       }

                                       function multiply(a, b) {
                                           return a * b;
                                       }

                                       function divide(a, b) {
                                           if (b == 0) {
                                               throw 'Division by zero';
                                           }
                                           return a / b;
                                       }

                                       function calculate(a, b) {
                                           let sum = add(a, b);
                                           let difference = subtract(a, b);
                                           let product = multiply(a, b);
                                           let quotient = divide(a, b);
                                       
                                           return {
                                               'sum': sum,
                                               'difference': difference,
                                               'product': product,
                                               'quotient': quotient
                                           };
                                       }

                                       function run() {
                                           let a = 10;
                                           let b = 2;
                                       
                                           let result = calculate(a, b);
                                       
                                           let finalResult = result.sum * result.difference / result.quotient;
                                       
                                           return finalResult;
                                       }
                                       """;

    internal static readonly JObject _v1LoopSettings = JObject.FromObject(
        new ExecuteScriptSettings
        {
            OutputVariable = "result", Function = "run", Source = LOOP_SCRIPT
        });

    internal static readonly JObject _v2LoopSettings = JObject.FromObject(
        new ExecuteScriptV2Settings
        {
            OutputVariable = "result", Function = "run", Source = LOOP_SCRIPT
        });

    internal static readonly JObject _v1MathSettings = JObject.FromObject(
        new ExecuteScriptSettings
        {
            OutputVariable = "result", Function = "run", Source = MATH_SCRIPT
        });

    internal static readonly JObject _v2MathSettings = JObject.FromObject(
        new ExecuteScriptV2Settings
        {
            OutputVariable = "result", Function = "run", Source = MATH_SCRIPT
        });

    internal static readonly JObject _v1JsonSettings = JObject.FromObject(
        new ExecuteScriptSettings
        {
            OutputVariable = "result", Function = "run", Source = JSON_SCRIPT
        });

    internal static readonly JObject _v2JsonSettings = JObject.FromObject(
        new ExecuteScriptV2Settings
        {
            OutputVariable = "result", Function = "run", Source = JSON_SCRIPT
        });

    internal static readonly JObject _v1SimpleSettings = JObject.FromObject(
        new ExecuteScriptSettings
        {
            OutputVariable = "result", Function = "run", Source = SIMPLE_SCRIPT
        });

    internal static readonly JObject _v2SimpleSettings = JObject.FromObject(
        new ExecuteScriptV2Settings
        {
            OutputVariable = "result", Function = "run", Source = SIMPLE_SCRIPT
        });
}