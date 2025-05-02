using System;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Take.Blip.Builder.Actions.ProcessHttp;

namespace Take.Blip.Builder.Utils
{
    /// <summary>
    /// Class to allow replacements of sensistive informations
    /// </summary>
    public class SensitiveInfoReplacer : ISensitiveInfoReplacer
    {
        private const string DEFAULT_VALUE_FOR_SUPRESSED_STRING = "***";
        private const string PATTERN = @"\{\{secret\..*?\}\}";

        /// <summary>
        /// Method to allow remove credentials from trace collector, avoiding store authorization keys externally
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string ReplaceCredentials(string value)
        {
            if (value == null)
            {
                return value;
            }

            var httpSettings = JsonConvert.DeserializeObject<ProcessHttpSettings>(value, OrderedJsonSerializerSettingsContainer.Settings);

            if (httpSettings.Headers == null)
            {
                return value;
            }

            foreach (var item in httpSettings.Headers.Keys.ToList())
            {
                httpSettings.Headers[item] = DEFAULT_VALUE_FOR_SUPRESSED_STRING;
            }

            if(httpSettings.Body != null)
            {
                var maskedBody = Regex.Replace(httpSettings.Body, PATTERN, DEFAULT_VALUE_FOR_SUPRESSED_STRING);
                httpSettings.Body = maskedBody;
            }

            var adjustedValue = JsonConvert.SerializeObject(httpSettings, OrderedJsonSerializerSettingsContainer.Settings);
            return adjustedValue;
        }
    }
}
