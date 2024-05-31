using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.ClearScript;

namespace Take.Blip.Builder.Actions.ExecuteScriptV2.Functions
{
    /// <summary>
    /// Workaround for overload methods: https://github.com/microsoft/ClearScript/issues/432#issuecomment-1289007466
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [ExcludeFromCodeCoverage]
    public static class ContextExtensions
    {
        /// <inheritdoc cref="Context.SetVariableAsync(string, object, TimeSpan)"/>
        public static Task SetVariableAsync(this Context context, string key, object value,
            Undefined _)
        {
            return context.SetVariableAsync(key, value);
        }
    }
}