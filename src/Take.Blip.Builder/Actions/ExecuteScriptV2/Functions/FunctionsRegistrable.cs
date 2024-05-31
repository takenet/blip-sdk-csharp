using System;
using System.Threading;
using Microsoft.ClearScript;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Actions.ExecuteScriptV2.Functions
{
    /// <summary>
    /// FunctionsRegistrable that can be executed by the script engine.
    /// </summary>
    public static class FunctionsRegistrable
    {
        /// <summary>
        /// Registers the functions that can be executed by the script engine.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="settings"></param>
        /// <param name="httpClient"></param>
        /// <param name="context"></param>
        /// <param name="time"></param>
        /// <param name="cancellationToken"></param>
        public static void RegisterFunctions(this ScriptEngine engine,
            ExecuteScriptV2Settings settings,
            IHttpClient httpClient, IContext context,
            Time time,
            CancellationToken cancellationToken)
        {
            // Date and time manipulation
            engine.AddHostObject("time", time);
            engine.AddHostType(typeof(TimeExtensions));
            engine.AddHostType(typeof(TimeSpan));

            _setDateTimezone(engine);

            // Context access
            engine.AddHostObject("context", new Context(context, time, cancellationToken));
            engine.AddHostType(typeof(ContextExtensions));

            // Fetch API
            engine.AddHostObject("request",
                new Request(settings, httpClient, context, time, cancellationToken));
            engine.AddHostType(typeof(RequestExtensions));
            engine.AddHostType(typeof(Request.HttpResponse));
        }

        private static void _setDateTimezone(IScriptEngine engine)
        {
            engine.Execute(@"
Date.prototype.toDateString = function () {
    return time.dateToString(this, {format: 'ddd MMM dd yyyy'});
};

Date.prototype.toTimeString = function () {
    return time.dateToString(this, {format: 'HH:mm:ss \'GMT\'zzz'});
};

Date.prototype.toString = function () {
    return this.toDateString() + ' ' + this.toTimeString();
};
");
        }
    }
}