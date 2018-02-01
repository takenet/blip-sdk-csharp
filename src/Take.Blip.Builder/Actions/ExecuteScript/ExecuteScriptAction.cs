using System;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Parser;
using Jint.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions.ExecuteScript
{
    public class ExecuteScriptAction : IAction
    {
        private const string DEFAULT_FUNCTION = "run";

        public string Type => nameof(ExecuteScript);

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var executeScriptSettings = settings.ToObject<ExecuteScriptSettings>();
            if (string.IsNullOrEmpty(executeScriptSettings.Source))
            {
                throw new ArgumentException($"The '{nameof(ExecuteScriptSettings.Source)}' settings value is required for '{nameof(ExecuteScriptSettings)}' action");
            }
            if (string.IsNullOrEmpty(executeScriptSettings.OutputVariable))
            {
                throw new ArgumentException($"The '{nameof(ExecuteScriptSettings.OutputVariable)}' settings value is required for '{nameof(ExecuteScriptSettings)}' action");
            }

            // Retrive the input variables
            object[] arguments = null;
            if (executeScriptSettings.InputVariables != null && executeScriptSettings.InputVariables.Length > 0)
            {
                arguments = new object[executeScriptSettings.InputVariables.Length];
                for (int i = 0; i < arguments.Length; i++)
                {
                    arguments[i] =
                        await context.GetVariableAsync(executeScriptSettings.InputVariables[i], cancellationToken);
                }
            }

            var engine = new Engine(options => options
                .LimitRecursion(5)
                .MaxStatements(50)
                .TimeoutInterval(TimeSpan.FromSeconds(2)))
                .Execute(executeScriptSettings.Source);

            var result = arguments != null 
                ? engine.Invoke(executeScriptSettings.Function ?? DEFAULT_FUNCTION, arguments) 
                : engine.Invoke(executeScriptSettings.Function ?? DEFAULT_FUNCTION);

            if (result != null && !result.IsNull())
            {
                var value = result.Type == Types.Object
                    ? GetJson(result.AsObject()).ToString(Formatting.None)
                    : result.ToString();

                await context.SetVariableAsync(executeScriptSettings.OutputVariable, value, cancellationToken);
            }
            else
            {
                await context.DeleteVariableAsync(executeScriptSettings.OutputVariable, cancellationToken);
            }
        }

        private static JToken GetJson(JsValue jsValue)
        {
            if (jsValue == null || jsValue.IsNull()) return null;
            if (jsValue.IsString()) return jsValue.AsString();            
            if (jsValue.IsBoolean()) return jsValue.AsBoolean();
            if (jsValue.IsNumber()) return jsValue.AsNumber();
            if (jsValue.IsDate()) return jsValue.AsDate().ToDateTime();
            if (jsValue.IsArray())
            {
                var jArray = new JArray();
                foreach (var keyValuePair in jsValue.AsArray().GetOwnProperties())
                {
                    jArray.Add(GetJson(keyValuePair.Value.Value));
                }

                return jArray;
            }
            if (jsValue.IsObject())
            {
                var jObject = new JObject();
                foreach (var keyValuePair in jsValue.AsObject().GetOwnProperties())
                {
                    jObject[keyValuePair.Key] = GetJson(keyValuePair.Value.Value);
                }

                return jObject;
            }

            return null;
        }
    }
}
