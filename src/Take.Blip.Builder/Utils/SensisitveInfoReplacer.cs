using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Utils
{
    /// <summary>
    /// Class to allow replacements of sensistive informations
    /// </summary>
    public class SensisitveInfoReplacer : ISensisitveInfoReplacer
    {
        private static readonly Regex KeyHeadersRegex = new Regex(@"^Key(((\r?\n|\s)*[A-Za-z0-9+\/]){4})*(((\r?\n|\s)*[A-Za-z0-9+\/]){2}(=(\r?\n|\s)*){2}|((\r?\n|\s)*[A-Za-z0-9+\/]){3}(=(\r?\n|\s)*))?$", RegexOptions.Compiled, Constants.REGEX_TIMEOUT);
        private const string DEFAULT_VALUE_FOR_SUPRESSED_STRING = "<Supressed Value>";

        /// <summary>
        /// Method to allow remove credentials from trace collector, avoiding store authorization keys externally
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string ReplaceCredentials(string value)
        {
            value = KeyHeadersRegex.Replace(value, DEFAULT_VALUE_FOR_SUPRESSED_STRING);
            return value;
        }
    }
}
