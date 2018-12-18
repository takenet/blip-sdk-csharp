using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.ProcessHttp
{
    public class ProcessHttpSettings : IValidable
    {
        public string Method { get; set; }

        public Uri Uri { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string Body { get; set; }

        public string ResponseBodyVariable { get; set; }

        public string ResponseStatusVariable { get; set; }

        public TimeSpan? RequestTimeout { get; set; }

        public void Validate()
        {
            if (Uri == null)
            {
                throw new ValidationException(
                    $"The '{nameof(Uri)}' settings value is required for '{nameof(ProcessHttpAction)}' action");
            }

            if (Method == null)
            {
                throw new ValidationException(
                    $"The '{nameof(Method)}' settings value is required for '{nameof(ProcessHttpAction)}' action");
            }
        }
    }
}