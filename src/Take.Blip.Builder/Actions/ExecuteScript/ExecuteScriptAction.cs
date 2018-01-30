using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Native.Json;
using Jint.Parser;
using Jint.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions.ExecuteScript
{
    public class ExecuteScriptAction : IAction
    {
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

            // Warning: Synchronous calls in an async context should be avoided.
            var engine = new Engine(options => options
                    .LimitRecursion(1)
                    .MaxStatements(5)
                    .TimeoutInterval(TimeSpan.FromSeconds(2)))
                .SetValue("getVariable",
                    new Func<string, string>(name => context.GetVariableAsync(name, cancellationToken).Result))
                .SetValue("setVariable",
                    new Action<string, string>((name, value) =>
                        context.SetVariableAsync(name, value, cancellationToken).Wait(cancellationToken)))
                .SetValue("setVariable",
                    new Action<string, string, double>((name, value, expiration) =>
                        context.SetVariableAsync(name, value, cancellationToken, TimeSpan.FromSeconds(expiration)).Wait(cancellationToken)))
                .SetValue("deleteVariable",
                    new Action<string>(name => context.DeleteVariableAsync(name, cancellationToken).Wait(cancellationToken)));

            var result = engine.Execute(executeScriptSettings.Source, new ParserOptions() {Tolerant = true}).GetCompletionValue();
            if (result != null && 
                !string.IsNullOrEmpty(executeScriptSettings.Variable))
            {
                string value;
                if (result.Type == Types.Object)
                {
                    value = GetJson(result.AsObject()).ToString(Formatting.None);                    
                }
                else
                {
                    value = result.ToString();
                }

                await context.SetVariableAsync(executeScriptSettings.Variable, value, cancellationToken);
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
