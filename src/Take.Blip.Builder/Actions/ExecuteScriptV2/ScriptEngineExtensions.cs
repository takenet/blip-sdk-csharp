using System;
using System.Threading;
using Microsoft.ClearScript;

namespace Take.Blip.Builder.Actions.ExecuteScriptV2
{
    /// <summary>
    /// Extensions for <see cref="ScriptEngine"/>.
    /// </summary>
    public static class ScriptEngineExtensions
    {
        private const string DEFAULT_FUNCTION = "run";

        /// <summary>
        /// Evaluates the specified code with a timeout.
        /// </summary>
        /// <param name="engine">The script engine.</param>
        /// <param name="code">The code to evaluate.</param>
        /// <param name="function"> The function to evaluate.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static object ExecuteInvoke(this ScriptEngine engine, string code,
            string function = "run",
            TimeSpan? timeout = null, params object[] args)
        {
            using var timer = new Timer(_ => engine.Interrupt());

            try
            {
                timer.Change(timeout ?? TimeSpan.FromSeconds(5),
                    TimeSpan.FromMilliseconds(Timeout.Infinite));

                engine.Execute(code);

                var result = args != null
                    ? engine.Invoke(function ?? DEFAULT_FUNCTION, args)
                    : engine.Invoke(function ?? DEFAULT_FUNCTION);

                return result;
            }
            catch (ScriptInterruptedException ex)
            {
                throw new TimeoutException("Script execution timed out", ex);
            }
        }
    }
}