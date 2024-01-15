using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Actions.ProcessHttp;
using Take.Blip.Builder.Models;
using Take.Blip.Client.Activation;

namespace Take.Blip.Builder.Utils
{
    /// <summary>
    /// Class to allow replacements of sensistive informations
    /// </summary>
    public class SensitiveInfoReplacer : ISensitiveInfoReplacer
    {
        private const string DEFAULT_VALUE_FOR_SUPRESSED_STRING = "***";

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

            var adjustedValue = JsonConvert.SerializeObject(httpSettings, OrderedJsonSerializerSettingsContainer.Settings);
            return adjustedValue;
        }
    }
}
