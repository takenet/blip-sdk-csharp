using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ClearScript;
using Newtonsoft.Json;
using Take.Blip.Builder.Actions.ExecuteScriptV2.Functions;

namespace Take.Blip.Builder.Actions.ExecuteScriptV2
{
    /// <summary>
    /// Utility converter to convert javascript result to c# representation on string.
    /// </summary>
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    public static class ScriptObjectConverter
    {
        /// <summary>
        /// Converts the data to string representation.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="time"></param>
        /// <param name="engine"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<string> ToStringAsync(
            object data,
            Time time,
            ScriptEngine engine,
            CancellationToken cancellationToken
        )
        {
            var resultData = await ConvertAsync(data, time, engine, cancellationToken);

            return resultData switch
            {
                DateTime dateTime => time.DateToString(dateTime),
                DateTimeOffset dateTime => time.DateOffsetToString(dateTime),
                string str => str,
                double @double => @double.ToString("R"),
                long @long => @long.ToString(),
                int @int => @int.ToString(),
                float @float => @float.ToString("R"),
                bool @bool => @bool ? "true" : "false",
                _ => JsonConvert.SerializeObject(resultData),
            };
        }

        /// <summary>
        /// Converts script result to c# object.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="time"></param>
        /// <param name="engine"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<object> ConvertAsync(
            object data,
            Time time,
            ScriptEngine engine,
            CancellationToken cancellationToken
        )
        {
            try
            {
                switch (data)
                {
                    case DateTime dateTime:
                        return time.DateToString(dateTime);

                    case DateTimeOffset dateTimeOffset:
                        return time.DateOffsetToString(dateTimeOffset);

                    case ScriptObject scriptObject when scriptObject.PropertyNames.Any():
                        return await ToDictionary(scriptObject, time, engine, cancellationToken);

                    case ScriptObject scriptObject:
                        return scriptObject.PropertyIndices.Any()
                            ? await ToList(scriptObject, time, engine, cancellationToken)
                            : data;

                    case Task<dynamic> task:
                    {
                        var delayTask = Task.Delay(Timeout.Infinite, cancellationToken);
                        var completedTask = await Task.WhenAny(task, delayTask);

                        if (completedTask == delayTask)
                        {
                            // Correção #1: interrompe o V8 e AGUARDA a task original
                            // terminar antes de propagar o cancelamento — nunca abandona.
                            engine.Interrupt();

                            try
                            {
                                await task.ConfigureAwait(false);
                            }
                            catch
                            {
                                // Já vamos propagar OperationCanceledException abaixo;
                                // aqui só garantimos que a task foi observada e que o
                                // engine não será descartado com ela ainda pendente.
                            }

                            throw new OperationCanceledException(cancellationToken);
                        }

                        if (completedTask.IsFaulted)
                        {
                            throw new ScriptEngineException(
                                "An error occurred while executing the script.",
                                task.Exception
                            );
                        }

                        if (completedTask.IsCanceled)
                        {
                            throw new OperationCanceledException(
                                "The script execution was canceled."
                            );
                        }

                        return await ConvertAsync(
                            ((Task<dynamic>)completedTask).Result,
                            time,
                            engine,
                            cancellationToken
                        );
                    }
                    default:
                        return data;
                }
            }
            catch (ObjectDisposedException ex)
            {
                throw new ScriptEngineException("Can not access disposed variable", ex);
            }
        }

        private static async Task<List<object>> ToList(
            ScriptObject scriptObject,
            Time time,
            ScriptEngine engine,
            CancellationToken cancellationToken
        )
        {
            var indexes = scriptObject.PropertyIndices.ToList();
            var results = new List<object>();

            foreach (var index in indexes)
            {
                results.Add(
                    await ConvertAsync(
                        scriptObject.GetProperty(index),
                        time,
                        engine,
                        cancellationToken
                    )
                );
            }

            return results;
        }

        private static async Task<Dictionary<string, object>> ToDictionary(
            ScriptObject scriptObject,
            Time time,
            ScriptEngine engine,
            CancellationToken cancellationToken
        )
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var propertyName in scriptObject.PropertyNames)
            {
                dictionary[propertyName] = await ConvertAsync(
                    scriptObject.GetProperty(propertyName),
                    time,
                    engine,
                    cancellationToken
                );
            }

            return dictionary;
        }
    }
}
