using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ClearScript;

namespace Take.Blip.Builder.Actions.ExecuteScriptV2.Functions
{
    /// <summary>
    /// Workaround for overload methods: https://github.com/microsoft/ClearScript/issues/432#issuecomment-1289007466
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [ExcludeFromCodeCoverage]
    public static class TimeExtensions
    {
        /// <inheritdoc cref="Time.DateToString(DateTime,IScriptObject)"/>
        public static string DateToString(this Time time, DateTime date, Undefined _)
        {
            return time.DateToString(date);
        }

        /// <inheritdoc cref="Time.ParseDate(string, IScriptObject)"/>
        public static DateTime ParseDate(this Time time, string date, Undefined _)
        {
            return time.ParseDate(date);
        }
    }
}